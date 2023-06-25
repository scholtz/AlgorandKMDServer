ver=3.13.1
image=$ver-stable
docker build -t scholtz2/algorand-kmd-mainnet-extended:$image -f Dockerfile --progress=plain --build-arg ALGO_VER=$ver --build-arg ALGO_BASE=$image . || error_code=$?
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

