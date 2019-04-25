import os
import sys
import traci
import traci.constants as tc

from traci import connection
from traci._vehicle import VehicleDomain
from traci._lane import LaneDomain

if 'SUMO_HOME' in os.environ:
    tools = os.path.join(os.environ['SUMO_HOME'], 'tools')
    sys.path.append(tools)
else:
    sys.exit("please declare environment variable 'SUMO_HOME'")


class ConnectionNotInitiatedException(Exception):
    pass


class SumoClient:
    def __init__(self, host="localhost", port=9999, show_gui=False, cfg_folder_path=".", step_length="0.01",
                 num_connections=1):
        """
        :type host:str
        :type port:int
        :type show_gui:bool
        :type cfg_folder_path:str
        :param host: The host to start up the sumo server on
        :param port: The port start up the sumo server on
        :param show_gui: Check to show sumo-gui
        :param cfg_folder_path: Specify the folder that contains configuration files
        :param step_length length of each step in the sumo simulation
        """
        self.__host = host
        self.__port = port
        self.__show_gui = show_gui
        self.__folder_path = cfg_folder_path
        self.__step_length = step_length
        self.__con = None
        self.__con_label = "sim1"
        self.__step = 0
        self.__num_connections = num_connections
        self.__vehicles_in_current_step = list()
        self.__vehicle_subscription_results = dict()
        self.__is_sumo_served = False;
        if not os.path.isabs(cfg_folder_path):
            self.__folder_path = os.path.abspath(cfg_folder_path)

        is_cfg_found = False
        file_found = ""
        for f in os.listdir(self.__folder_path):
            if f.endswith(".sumo.cfg") or f.endswith(".sumocfg"):
                is_cfg_found = True
                file_found = f
                break

        if is_cfg_found:
            print("[ Sumo Configuration ( .cfg ) file found is: < " + file_found + ">]")
            self.__cfg_file = file_found
        else:
            raise ValueError('Cfg folder provided does not contain sumo configuration file',
                             'Folder provided: ' + self.__folder_path)

    def serve(self):
        """
        Serves Sumo server and connects the SumoClient with it.
        If serve is called no call to connect is needed.
        """
        if self.__show_gui:
            sumo_binary = "sumo-gui"
        else:
            sumo_binary = "sumo"

        sumo_cmd = [sumo_binary, "-c", self.__cfg_file]
        if self.__num_connections > 1:
            sumo_cmd.extend(["--num-clients", str(self.__num_connections)])
            print(sumo_cmd)
        traci.start(sumo_cmd, port=self.__port, numRetries=10, label=self.__con_label)
        self.__is_sumo_served = True
        self.connect()

    def connect(self):
        """
        If called after calling serve then it just returns the connection.
        If serve is not called then it attempts to connect to existing sumo server
        and then return the connection.
        :rtype: connection
        :return: The connection to sumo server.
        """
        if self.__is_sumo_served:
            self.__con = traci.getConnection(label=self.__con_label)
        else:
            self.__con = traci.connect(port=self.__port, numRetries=10, host=self.__host, proc=None)

    def get_connection(self):
        """
        :rtype: connection
        :return: The connection made.
        :raises: ConnectionNotInitiatedException if SumoClient didn't connect.
        """
        if self.__con is None:
            if self.__is_sumo_served:
                self.connect()
            else:
                raise ConnectionNotInitiatedException("Cannot get connection because connection"
                                                      " has not yet been initialised")
        return self.__con

    def make_simulation_step(self, step=0):
        """
        :type step: int
        :param step: The number of simulation steps to make
        :return:
        """

        if step == 0:
            self.__step += 1
        else:
            self.__step += step
        self.__con.simulationStep(step)
        self.__con.simulationStep(step)
        departed_vehicles = self.__con.simulation.getDepartedIDList()
        arrived_vehicles = self.__con.simulation.getArrivedIDList()

        self.__vehicles_in_current_step.extend(departed_vehicles)

        for departedVehicle in departed_vehicles:
            self.__con.vehicle.subscribe(objectID=departedVehicle, varIDs=(tc.VAR_POSITION, tc.VAR_ANGLE, tc.VAR_SPEED,
                                                                      tc.VAR_ACCEL, tc.VAR_SIGNALS))

        for v in self.__vehicles_in_current_step:
            v_subscription_results = self.__con.vehicle.getSubscriptionResults(objectID=v)
            if v_subscription_results is None:
                del self.__vehicle_subscription_results[v]
                self.__con.vehicle.unsubscribe(v)
            else:
                self.__vehicle_subscription_results[v] = self.__con.vehicle.getSubscriptionResults(objectID=v)

    def get_current_step_count(self):
        """
        :return: The count of steps executed
        :rtype: int
        """
        return self.__step

    def get_vehicles_subscription_results(self):
        """
        :return: A dict for the subscription results of all vehicles
                 key is the id of the vehicle and value is the subscription
                 result dict.
                 None if no vehicles are in the current simulation step.
        :rtype: dict
        """
        return self.__vehicle_subscription_results

    def get_vehicle_subscription_result(self, vehicle_id):
        """
        :type vehicle_id:str
        :param vehicle_id: The id of the vehicle to get the subscription results of
        :return: dict of the code of subscribed value and the result.
                 key is the code of the subscribed value and value is the result.
                 None if there is no such vehicle in the dict.(That means that the vehicle
                 doesn't exist in the current simulation step.
        :rtype: dict
        """
        return self.__vehicle_subscription_results.get(vehicle_id)

    def get_vehicle_ids(self):
        """
        :return: A list of all the vehicles that exist in the current step
        :rtype: list
        """
        return self.__vehicle_subscription_results.keys()

    def set_speed(self, vehicle_id, new_speed):
        """
        :type vehicle_id: str
        :type new_speed:
        :param vehicle_id: The id of the vehicle to change the speed of
        :param new_speed: The new speed of the vehicle
        :return:
        """
        self.__con.vehicle.setSpeed(vehicle_id, new_speed)

    def vehicle_commands(self):
        """
        :rtype: VehicleDomain
        :return: Object for vehicle commands.
        """
        return self.__con.vehicle

    def lane_commands(self):
        """
        :rtype: LaneDomain
        :return: Object for lane commands.
        """
        return self.__con.lane

    def set_order(self, order):
        """
        :type order: int
        :param order:
        """
        self.__con.setOrder(order)

    def get_version(self):
        return self.__con.getVersion()

    def step_no_change(self):
        self.__con.simulationStep(-1)


def start_connection(host="localhost", port=9999, show_gui=False, cfg_folder_path=".", step_length="0.01",
                     num_connections=1, connection_order=1, serve_sumo=True):
    """
    :type host:str
    :type port:int
    :type show_gui:bool
    :type cfg_folder_path:str
    :type num_connections:int
    :type connection_order:int
    :type serve_sumo: bool
    :param host: The host to start up the sumo server on
    :param port: The port start up the sumo server on
    :param show_gui: Check to show sumo-gui
    :param cfg_folder_path: Specify the folder that contains configuration files
    :param step_length length of each step in the sumo simulation
    :param num_connections: How many connections on the server
    :param connection_order: Must specify connection order if num_connections > 1
    :param serve_sumo: Check to also serve sumo
    :returns The SumoClient object
    """
    sumo_client = SumoClient(host, port, show_gui, cfg_folder_path, step_length, num_connections)
    if serve_sumo:
        sumo_client.serve()
    sumo_client.connect()
    if num_connections > 1:
        sumo_client.set_order(connection_order)
    return sumo_client
