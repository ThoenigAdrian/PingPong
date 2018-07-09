import socket
import os
import json

open_session = {"PackageType":45645, "Reconnect":True, "ReconnectSessionID":"YouSuck"}

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
        bla_to_send = json.dumps(open_session)
        s.send(chr(0) + chr(len(bla_to_send)) + 3*chr(0) + bla_to_send)
    s.close()

open_many_tcp_session_then_close_them_all()
#open_many_tcp_session_instantly_close(number_of_session=1000)
#fuzz_random_bytes(100)

