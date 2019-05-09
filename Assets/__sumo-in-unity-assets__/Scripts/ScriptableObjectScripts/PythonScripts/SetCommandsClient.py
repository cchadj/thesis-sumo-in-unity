import sys
import os
import traci
import SumoClient
import socket
import SocketWrapper
import CommandMessage_pb2
import CollectVehicleInfoViaSubscription
from MySocketServer import TcpServer
import traci.constants as tc
from MySocketServer import ServerState


class SetCommandClient(TcpServer):
    def __init__(self, host='localhost', port=9999, sumo_host='localhost', sumo_port=25000):
        TcpServer.__init__(self, host, port)
        self.sumo_host, self.sumo_port = sumo_host, sumo_port
        self.__sumo_client = SumoClient.start_connection(host=self.sumo_host, port=self.sumo_port, serve_sumo=False,
                                                         num_connections=2, connection_order=2)
        self.messages = None
        self.__client_socket_wrapper = None

    def serve(self):
        # create an INET, STREAMing socket
        # bind the socket to a public host, and a well-known port
        self.server_socket.bind((self.host, self.port))
        # become a server socket
        self.server_socket.listen(5)
        # accept connections from outside
        # Currently accept only one connection
        (client_socket, address) = self.server_socket.accept()
        self.__client_socket_wrapper = SocketWrapper.socket_wrapper(client_socket)
        while True:
            self.handle_request()

    def handle_request(self):
        messageList = self.__client_socket_wrapper.receive_proto(CommandMessage_pb2.CommandList)
        self.handle_messages(messageList)

    def handle_messages(self, commandList):
        """
        :type messages: CommandMessage_pb2.CommandList
        :param messages: The list of messages to handle/execute and send to sumo server.
        """
        for message in commandList.messages:
            self.handle_message(self, message)
        self.__sumo_client.get_connection().simulationStep(-1)

    def handle_message(self, message):
        """
        :type message: CommandMessage_pb2.CommandMessage
        :param message: The list of messages to handle/execute and send to sumo server.
        TODO Handle errors and implement more commands.
        """
        command = message.cmd_var_bytes[0]
        variable = message.cmd_var_bytes[1]
        if command == tc.CMD_SET_VEHICLE_VARIABLE:
            if variable == tc.VAR_SPEED:
                self.__sumo_client.vehicle_commands().setSpeed(message.id, message.float_value)
        self.sent_data += "1"


def create_client():
    return SetCommandClient()


if __name__ == "__main__":
    CollectVehicleInfoViaSubscription.start(num_connections=2, serve_sumo=False, order=1)
