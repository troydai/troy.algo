echo "This is sample1.sh"

src=${1//\\//}
dst=${2//\\//}

echo "Argument 1: $1 => $src"
echo "Argument 2: $2 => $dst"
