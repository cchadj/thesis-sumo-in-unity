import libsumo as traci
import sys
import traci.constants as tc
from msvcrt import getch
import time

active_vehicles = []


def print_vehicles():
    print("Departed Vehicles : " + str(traci.simulation.getDepartedIDList()))
    print("Arrived  Vehicles : " + str(traci.simulation.getArrivedIDList()))
    print("Loaded   Vehicles : " + str(traci.simulation.getLoadedIDList()))


# class SimulationListener(traci.StepListener):
#     def step(self, t=0):
#         print("Hello there alligator")
#         return True

traci.start(["sumo", "-c", "run2500.sumocfg", "--step-length", "0.05"])

i = 0
start = time.time()
while traci.simulation.getMinExpectedNumber() > 0:

    if i == 1000:
        break
    i += 1

    for veh_id in traci.simulation.getDepartedIDList():
        traci.vehicle.subscribe(veh_id, [traci.constants.VAR_POSITION])
    positions = traci.vehicle.getSubscriptionResults("0")

    traci.simulationStep()

end = time.time()
print("execution-time: " + str(end-start))
exit(0)
# 16.168025493621826 33.48400807380676

message = r"""
               ************************************************************
                * Press:                                                   *
                *   Enter - to make a Simulation Step                      *
                *   V - to make Variable Subscription                      *
                *   C - to make Context Subscription                       *
                *   P - to print Vehicles                                  *
                *   T - to print Simulation Time                           *
                *   Q - to stop the simulation                             *
                *   H - to print this instruction menu                     *
                *   L - to create logs and plots for performance test      *
                ************************************************************
"""
print(message)

traci.simulationStep()
# simulationListener = SimulationListener()
traci.simulation.subscribe("ignored", [tc.VAR_DEPARTED_VEHICLES_IDS, tc.VAR_ARRIVED_VEHICLES_IDS])
# traci.addStepListener(simulationListener)
while True:
    print("Press enter to make a simulation step")
    pressedKey = getch()
    if pressedKey == b'h':
        print(message)
    elif pressedKey == b'q':
        print("Ending Simulation")
        break
    elif pressedKey == b'p':
        print_vehicles()
    elif pressedKey == b'x':
        sys.exit()
    elif pressedKey == b'\r':
        traci.simulationStep()
        print()

print("Simulation ended")
