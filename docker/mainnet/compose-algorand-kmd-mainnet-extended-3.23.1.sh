ver=3.23.1
image=$ver-stable
Application="algorand-participation-server"
dockerImage="scholtz2/algorand-kmd-mainnet-extended"
version=$image

gitVer=`git rev-parse HEAD`

echo "{\"applicationName\":\"$Application\",\"buildNumber\":\"$ver\",\"dllVersion\":\"\",\"buildTime\":\"$time\",\"sourceVersion\":\"$gitVer\",\"dockerImage\":\"$dockerImage:$version\",\"dockerImageVersion\":\"$version\"}"> "version.json" 
cat "version.json" 

docker build -t scholtz2/algorand-kmd-mainnet-extended:$image -f Dockerfile --build-arg ALGO_VER=$ver --build-arg ALGO_BASE=$image ../../ || error_code=$?
error_code_int=$(($error_code + 0))
if [ $error_code_int -ne 0 ]; then
    echo "failed to build";
	exit 1;
fi

docker push scholtz2/algorand-kmd-mainnet-extended:$ver-stable || error_code=$?
error_code_int=$(($error_code + 0))
if [ $error_code_int -ne 0 ]; then
    echo "failed to push algorand-kmd-mainnet-extended";
	exit 1;
fi


docker tag scholtz2/algorand-kmd-mainnet-extended:$ver-stable scholtz2/algorand-participation-mainnet-extended:$ver-stable
docker push scholtz2/algorand-participation-mainnet-extended:$ver-stable || error_code=$?
error_code_int=$(($error_code + 0))
if [ $error_code_int -ne 0 ]; then
    echo "failed to push algorand-participation-mainnet-extended";
	exit 1;
fi