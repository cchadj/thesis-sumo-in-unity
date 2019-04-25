#!/usr/bin/env sh
POSH="hello"
cat <<'EOF' > "$POSH/run1.sumocfg"
<configuration>
    <input>
        <net-file value="net.net.xml"/>
        <route-files value="rou1.rou.xml"/>
    </input>
</configuration>
EOF

cat <<'EOF' > 'run5.sumocfg'
<configuration>
    <input>
        <net-file value="net.net.xml"/>
        <route-files value="rou5.rou.xml"/>
    </input>
</configuration>
EOF

cat <<'EOF' > 'run10.sumocfg'
<configuration>
    <input>
        <net-file value="net.net.xml"/>
        <route-files value="rou10.rou.xml"/>
    </input>
</configuration>
EOF


cat <<'EOF' > 'run50.sumocfg'
<configuration>
    <input>
        <net-file value="net.net.xml"/>
        <route-files value="rou50.rou.xml"/>
    </input>
</configuration>
EOF

cat <<'EOF' > 'run100.sumocfg'
<configuration>
    <input>
        <net-file value="net.net.xml"/>
        <route-files value="rou100.rou.xml"/>
    </input>
</configuration>
EOF

cat <<'EOF' > 'run250.sumocfg'
<configuration>
    <input>
        <net-file value="net.net.xml"/>
        <route-files value="rou250.rou.xml"/>
    </input>
</configuration>
EOF

cat <<'EOF' > 'run500.sumocfg'
<configuration>
    <input>
        <net-file value="net.net.xml"/>
        <route-files value="rou500.rou.xml"/>
    </input>
</configuration>
EOF

cat <<'EOF' > 'run750.sumocfg'
<configuration>
    <input>
        <net-file value="net.net.xml"/>
        <route-files value="rou750.rou.xml"/>
    </input>
</configuration>
EOF

cat <<'EOF' > 'run1000.sumocfg'
<configuration>
    <input>
        <net-file value="net.net.xml"/>
        <route-files value="rou1000.rou.xml"/>
    </input>
</configuration>
EOF

cat <<'EOF' > 'run2500.sumocfg'
<configuration>
    <input>
        <net-file value="net.net.xml"/>
        <route-files value="rou2500.rou.xml"/>
    </input>
</configuration>
EOF

cat <<'EOF' > 'run5000.sumocfg'
<configuration>
    <input>
        <net-file value="net.net.xml"/>
        <route-files value="rou5000.rou.xml"/>
    </input>
</configuration>
EOF

cat <<'EOF' > 'run7500.sumocfg'
<configuration>
    <input>
        <net-file value="net.net.xml"/>
        <route-files value="rou7500.rou.xml"/>
    </input>
</configuration>
EOF
