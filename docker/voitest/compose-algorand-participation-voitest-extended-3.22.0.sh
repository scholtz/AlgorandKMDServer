ver=3.22.0
image=$ver-stable
Application="algorand-participation-server"
dockerImage="scholtz2/algorand-participation-voitest-extended"
version=$image

gitVer=`git rev-parse HEAD`

echo "{\"applicationName\":\"$Application\",\"buildNumber\":\"$ver\",\"dllVersion\":\"\",\"buildTime\":\"$time\",\"sourceVersion\":\"$gitVer\",\"dockerImage\":\"$dockerImage:$version\",\"dockerImageVersion\":\"$version\"}"> "version.json" 
cat "version.json" 

docker build -t $dockerImage:$image -f Dockerfile --build-arg ALGO_VER=$ver --build-arg ALGO_BASE=$image ../../ || error_code=$?
error_code_int=$(($error_code + 0))
if [ $error_code_int -ne 0 ]; then
    echo "failed to build";
	exit 1;
fi
echo "docker push $dockerImage:$image"
docker push $dockerImage:$image || error_code=$?
error_code_int=$(($error_code + 0))
if [ $error_code_int -ne 0 ]; then
    echo "failed to push $dockerImage:$image";
	exit 1;
fi