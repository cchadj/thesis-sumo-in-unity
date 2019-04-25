#!/usr/bin/env sh

NET_XML_NAME="net.net.xml"
ARM_NUMBER="20"
CIRCLE_NUMBER="60"
RADIUS="25"
DIRECTORY_OUTPUT=""

while getopts ":n:a:c:d:r:" opt; do
  case $opt in
    n) NET_XML_NAME="$OPTARG" 
    ;;
    a) ARM_NUMBER="$OPTARG"
    ;;
    c) CIRCLE_NUMBER="$OPTARG"
    ;;
    d) DIRECTORY_OUTPUT="$OPTARG"
    ;;
    r) RADIUS="$OPTARG"
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

echo "Using net $NET_XML_NAME"
echo "Arm number $ARM_NUMBER"
echo "Circle number $CIRCLE_NUMBER"
echo "Space radius $RADIUS"

if [ ! -z $DIRECTORY_OUTPUT ]
then
    mkdir -p "$DIRECTORY_OUTPUT"
else
   echo "No Directory given"
   exit 1
fi

NETGENERATE.exe --spider \
                --spider.arm-number $ARM_NUMBER \
                --spider.circle-number $CIRCLE_NUMBER \
                --spider.space-radius $RADIUS \
                --spider.omit-center true \
                --lefthand


cp "$NET_XML_NAME" "$DIRECTORY_OUTPUT/" 

NUMBER_OF_VEHICLES_ARRAY=( 1 5 10 50 100 250 500 750 1000 1250 2500 3000)
for i in "${NUMBER_OF_VEHICLES_ARRAY[@]}"
do
    PERIOD=`echo "100/$i" | bc -l | tr -d '\n'`
    echo "Period:" "$PERIOD"
    echo "Number of vehicles: $i" 
    randomTrips.py -n $NET_XML_NAME -b 1 -e 101 -p $PERIOD   -r "$DIRECTORY_OUTPUT/"./rou"$i".rou.xml

    cat <<EOF > "$DIRECTORY_OUTPUT/run$i.sumocfg"
    <configuration>
        <input>
            <net-file value="net.net.xml"/>
            <route-files value="rou$i.rou.xml"/>
        </input>
    </configuration>
EOF
done
