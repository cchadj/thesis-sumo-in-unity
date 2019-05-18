#!/usr/bin/env sh

while getopts ":n:d:" opt; do
  case $opt in
    n) NET_XML_NAME="$OPTARG" 
    ;;
    d) DIRECTORY_OUTPUT="$OPTARG"
    ;;
    \?)
    echo "Invalid option: -$OPTARG" >&2
    exit 1
    ;;
    :)
      echo "Option -$OPTARG requires an argument." >&2
      exit 1
      ;;
  esac
done

if [ -z $NET_XML_NAME ]
then
   echo "No .net.xml given"
   exit 1
fi

if [ ! -f $NET_XML_NAME ]
then
   echo "No such net.xml file '$NET_XML_NAME'"
   exit 1
fi

if [ ! -z $DIRECTORY_OUTPUT ]
then
    mkdir -p "$DIRECTORY_OUTPUT"
else
   echo "No Directory given"
   exit 1
fi

echo "Using net $NET_XML_NAME"
echo "Resulting .sumo.cfg and rout.rou files at $DIRECTORY_OUTPUT"


cp "$NET_XML_NAME" "$DIRECTORY_OUTPUT/" 

# Generate sumo.cfg for each rumber of vehicles based on NET_XML_NAME 
NUMBER_OF_VEHICLES_ARRAY=(1 5 10 25 50  75 100 150 250 275 300 475 500)
for i in "${NUMBER_OF_VEHICLES_ARRAY[@]}"
do
    PERIOD=`echo "100/$i" | bc -l | tr -d '\n'`
    echo "Period:" "$PERIOD"
    echo "Number of vehicles: $i" 
    randomTrips.py -n $NET_XML_NAME -b 1 -e 101 -p $PERIOD   -r "$DIRECTORY_OUTPUT/"./rou"$i".rou.xml

    cat <<EOF > "$DIRECTORY_OUTPUT/run$i.sumocfg"
    <configuration>
        <input>
            <net-file value="$NET_XML_NAME"/>
            <route-files value="rou$i.rou.xml"/>
        </input>
    </configuration>
EOF
done
