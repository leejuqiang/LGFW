#!/bin/sh
rm parseExcel.sh
echo '#!/bin/sh' >> parseExcel.sh
echo `which python3` "Assets/LGFW/Editor/parseExcel.py" '"$1"' >> parseExcel.sh
chmod +x parseExcel.sh