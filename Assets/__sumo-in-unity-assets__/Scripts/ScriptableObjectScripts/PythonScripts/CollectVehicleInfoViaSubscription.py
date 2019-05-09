import os
import sys
import traci
import traci.constants as tc
import SumoClient

def is_number(s):
    try:
        float(s)
        return True
    except ValueError:
        return False


def start( num_connections=2, serve_sumo=True, order=1):
    """
    :param num_connections:
    :param serve_sumo:
    :param order:
    :return:
    """
    sumo_client = SumoClient.start_connection(show_gui=True, num_connections=num_connections, serve_sumo=serve_sumo,
                                              connection_order=order)

    sumo_client.get_connection().simulationStep()
    while True:
        ans = raw_input(" Change Nothing and give turn ( 1 )\n "
                        "Make Simulation Step and Give Turn ( 2 ) \n"
                        " Move to Queries ( 3 )")
        if ans == "1":
            sumo_client.get_connection().simulationStep(-1)
            continue

        elif ans == "2":
            sumo_client.make_simulation_step()
            continue
        elif ans == "3":
            pass
        else:
            print("Select  1 - 3")
            continue
        vs_dict = sumo_client.get_vehicles_subscription_results()
        vehicles = sumo_client.get_vehicle_ids()
        # Control Commands
        print(sumo_client.get_version())
        new_speed = raw_input("Enter a float for new speed")

        if len(vehicles) < 1:
            continue
        try:
            sumo_client.set_speed(vehicles[0], float(new_speed))
        except ValueError:
            pass
        cur_veh_lane_id = sumo_client.vehicle_commands().getLaneID(vehicles[0])
        cur_veh_edge_id = sumo_client.lane_commands().getEdgeID(cur_veh_lane_id)
        cur_veh_route_id = sumo_client.vehicle_commands().getRoute(vehicles[0])
        if len(vehicles) > 0:
            print("vehicle queried for lane " + vehicles[0])
            print("vehicle is on lane" + cur_veh_lane_id)
            print ("Vehicle is on edge " + cur_veh_edge_id)
            print("Vehicle route ")
            print '[%s]' % ', '.join(map(str, cur_veh_route_id))
        answer_on_stop = raw_input("Do you want to stop the vehicle on this edge (yes for yes) " + vehicles[0])
        if answer_on_stop == "yes":
            answer = raw_input("Select route to stop")
            routeIndex = int()
            try:
                routeIndex = int(answer)
                sumo_client.vehicle_commands().setStop(vehicles[0],  cur_veh_route_id[routeIndex])
                print("Vehicle " + vehicles[0] + " will stop at " + cur_veh_route_id[routeIndex])
            except ValueError:
                print("Please enter a number")

        if raw_input("exit?") == "yes":
            sumo_client.get_connection().close()
        print("Current vehicle speed " + str((vs_dict[vehicles[0]])[tc.VAR_SPEED]))
        print("Max Acceleration " + str((vs_dict[vehicles[0]])[tc.VAR_ACCEL]))
