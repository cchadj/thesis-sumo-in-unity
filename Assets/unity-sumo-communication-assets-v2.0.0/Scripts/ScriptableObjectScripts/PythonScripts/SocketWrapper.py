import google.protobuf
import socket
from google.protobuf.internal.decoder import _DecodeVarint32
from google.protobuf.internal.encoder import _VarintBytes
import binascii


class SocketWrapper:
    """
    SocketWrapper provides some methods to send and receive data from the
    socket that this object wraps. Such methods are send_proto and receive_proto
    that work like WriteDelimitedTo and ReadDelimitedFrom the socket.
    """
    def __init__(self, sock=None):
        self.ENDMARKER = "THIS_IS_THE_END"
        self.PREFIX_SIZE = 4
        if sock is None:
            self.sock = socket.socket(
                            socket.AF_INET, socket.SOCK_STREAM)
        else:
            self.sock = sock

    def connect(self, host, port):
        self.sock.connect((host, port))

    def send(self, msg):
        """ :type msg: str"""
        size_prefix = format(len(msg), '0'+str(self.PREFIX_SIZE)+'d')
        new_message = size_prefix + msg
        print ("Message send is: " + new_message)
        return self.sock.send(new_message)

    # Assume that the first 4 bytes represent the number of bytes to read
    def receive(self):
        first_chunk = self.sock.recv(self.PREFIX_SIZE)
        bytes_to_read = int(first_chunk)

        chunks = []
        chunks.append(first_chunk)
        bytes_received = 0
        while bytes_received < bytes_to_read:
            chunk = self.sock.recv(2048)
            if chunk == b'':
                 raise RuntimeError("socket connection broken")
            chunks.append(chunk)
            bytes_received = bytes_received + len(chunk)

        final_message_received = b''.join(chunks)
        print("Message received is: " + final_message_received)
        return b''.join(chunks)

    def __socket_read_n(self, n):
        """
            :type n: int
            Read exactly n bytes from the socket.
            Raise RuntimeError if the connection closed before
            n bytes were read.
        """
        buf = ''
        while n > 0:
            data = self.sock.recv(n)
            if data == '':
                raise RuntimeError('unexpected connection close')
            buf += data
            n -= len(data)
        return buf

    def receive_proto(self, msgtype):
        """
            :type msgtype: google.protobuf.message
            Read a message from a socket. msgtype is a subclass of
            of protobuf Message.
            The message read must be delimited with the size in the front.
            :rtype: msgtype
        """
        header_bytes = self.__socket_read_n(4)
        (msg_length, header_length) = _DecodeVarint32(header_bytes, 0)
        response_bytes = ""
        # if header length is less that 4 then the rest of the bytes are response bytes
        if header_length < 4:
            response_bytes += header_bytes[header_length:]

        # read the remaining message bytes
        msg_length = msg_length - (4 - header_length)
        cur_response_bytes = ""
        while msg_length > 0:
            cur_response_bytes = self.sock.recv(min(8096, msg_length))
            msg_len = len(cur_response_bytes)
            msg_length -= msg_len
        response_bytes += cur_response_bytes
        # response_bytes += b'\x01'
        stringified = binascii.hexlify(bytearray(response_bytes))
        print ("The stringified proto in server: " + stringified)
        msg = msgtype()

        msg.ParseFromString(response_bytes)
        return msg

    def send_proto(self, imsg):
        """ :type imsg: google.protobuf.message
            Sends a proto imessage to the socket.
            Equal to WriteDelimitedTo (in c# and java)
        """
        serialized_msg = imsg.SerializeToString()
        stringified = binascii.hexlify(bytearray(serialized_msg))
        print ("The stringified proto in client: " + stringified)
        assert imsg.ByteSize() == len(serialized_msg)

        size = _VarintBytes(len(serialized_msg))
        if imsg.ByteSize() == len(serialized_msg):
            print("Correct " + str(int(size.encode('hex'), 16)) + str(imsg.ByteSize()) + str(len(serialized_msg)))
        msg_to_send = size + serialized_msg
        self.sock.send(msg_to_send)


def socket_wrapper(sock):
    """
    :type sock: socket
    :param sock: The socket to wrap the socket
    :return: the SocketWrapper for the socket provided
    """
    return SocketWrapper(sock)
