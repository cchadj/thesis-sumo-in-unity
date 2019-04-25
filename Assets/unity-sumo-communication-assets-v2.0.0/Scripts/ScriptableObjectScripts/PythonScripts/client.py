import socket
import car_pb2
import SocketWrapper

def client(string):
    HOST, PORT = 'localhost', 9999
    # SOCK_STREAM == a TCP socket
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    # sock.setblocking(0)  # optional non-blocking
    sock.connect((HOST, PORT))
    sw = SocketWrapper.socket_wrapper(sock)

    sw.send(string)
    print (sw.receive()) # limit reply to 16K

    car = car_pb2.car()
    car.id = "1"
    car.speed = 133
    sw.send_proto(car)
    print (sw.receive())


if __name__ == "__main__":
    client("car")
