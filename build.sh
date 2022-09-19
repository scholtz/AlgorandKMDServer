ver=3.9.4
docker build -t scholtz2/algorand-kmd-mainnet-extended:$ver-stable -f Dockerfile --progress=plain --build-arg ALGO_VER=$ver .
docker push scholtz2/algorand-kmd-mainnet-extended:$ver-stable
