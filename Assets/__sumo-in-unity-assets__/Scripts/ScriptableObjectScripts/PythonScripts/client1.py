import sys
import os
import traci
import CollectVehicleInfoViaSubscription


if __name__ == "__main__":
    CollectVehicleInfoViaSubscription.start(num_connections=2, serve_sumo=False, order=1)
