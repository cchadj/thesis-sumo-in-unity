import car_pb2
from enum import Enum
import socket
import SocketWrapper


class ServerState(Enum):
    EXPECTING_HELLO = 1
    EXPECTING_CAR = 2


class TcpServer:

    def __init__(self, host="localhost", port=9999):
        self.server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.host = host
        self.port = port
        self.state = ServerState.EXPECTING_HELLO
        self.received_data = ""
        self.sent_data = ""

    def serve(self):
        # create an INET, STREAMing socket
        # bind the socket to a public host, and a well-known port
        self.server_socket.bind((self.host, self.port))
        # become a server socket
        self.server_socket.listen(5)
        # accept connections from outside
        # Currently accept only one connection
        (client_socket, address) = self.server_socket.accept()
        socket_wrapper = SocketWrapper.socket_wrapper(client_socket)
        while True:
            print("[Current Server state is: " + str(self.state) + "]")

            if self.state is ServerState.EXPECTING_HELLO:
                self.received_data = socket_wrapper.receive()
                print("[Client from " + str(address) + " wrote " + self.received_data + "]")
                self.state = ServerState.EXPECTING_CAR
                self.sent_data = "Connection was successful. Hello from python.\n" \
                                 "[Server state is now " + str(self.state) + "]"
                print("[ Writing " + self.sent_data + " to client " + str(address))
                socket_wrapper.send(self.sent_data)
            elif self.state is ServerState.EXPECTING_CAR:
                received_car = socket_wrapper.receive_proto(car_pb2.car)
                self.sent_data = "Car received {id: " + received_car.id + ", speed:" + str(received_car.speed) + " }"
                socket_wrapper.send(self.sent_data)


if __name__ == "__main__":
    server = TcpServer()
    server.serve()
