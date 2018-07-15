import socket
import os
import json
import time

open_session = {"PackageType":4, "Reconnect":True, "ReconnectSessionID":"YouSuck"}

class TCPPacket(object):
    def __init__(self, string):
        if not isinstance(string, basestring):
            string = json.dumps(string)
        self.packet_content = chr(0) + chr(len(string)) + 3 * chr(0) + string

    def __str__(self):
        return self.packet_content

    def create_1KB_Package(self):
        self.packet_content = chr(0) + chr(0) + chr(4) + chr(0) + chr(0) + "{" + ("a" * (1023))


def open_many_tcp_session_then_close_them_all(number_of_session=1000):
    socket_list = []
    for i in range(number_of_session):
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.connect(("127.0.0.1", 4200))
        socket_list.append(s)
    for sock in socket_list:
        sock.close()

def open_many_tcp_session_instantly_close(number_of_session=1000):
    for i in range(number_of_session):
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.connect(("127.0.0.1", 4200))
        s.close()

def fuzz_random_bytes(number_of_packets=100):
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.connect(("127.0.0.1", 4200))
    for i in range(number_of_packets):
        s.send(str(TCPPacket(open_session)))
    s.close()

def server_slower():
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.connect(("127.0.0.1", 4200))

    One_KB_Packet = TCPPacket("dummy")
    One_KB_Packet.create_1KB_Package()

    TEN_MB = 1000 * 1000 * 1000
    print len((str(One_KB_Packet)))
    print TEN_MB/len(str(One_KB_Packet))
    for _ in range(TEN_MB/len(str(One_KB_Packet))):
        s.send(str(One_KB_Packet))

#open_many_tcp_session_then_close_them_all()
#open_many_tcp_session_instantly_close(number_of_session=1000)
#fuzz_random_bytes(1)
server_slower()

