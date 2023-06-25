ver=3.11.2
docker build -t scholtz2/algorand-kmd-mainnet-extended:$ver-stable -f Dockerfile --progress=plain --build-arg ALGO_VER=$ver . || error_code=$?
error_code_int=$(($error_code + 0))
if [ $error_code_int -ne 0 ]; then
    echo "failed to build";
	exit 1;
fi

docker push scholtz2/algorand-kmd-mainnet-extended:$ver-stable || error_code=$?
error_code_int=$(($error_code + 0))
if [ $error_code_int -ne 0 ]; then
    echo "failed to build";
	exit 1;
fi

