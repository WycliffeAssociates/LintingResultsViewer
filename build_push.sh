#!/bin/bash

BUILD_IMAGE_NAME=linting-results
if [ -z "$BUILD_IMAGE_NAME" ]; then 
    echo "ERROR: env var BUILD_IMAGE_NAME is blank or undefined"
    exit 1
fi

CURRENT_GIT_COMMIT=$(git rev-parse HEAD)

if [ -z "$JENKINS_HOME" ];
  then CURRENT_GIT_BRANCH=$(git rev-parse --abbrev-ref HEAD); 
  else CURRENT_GIT_BRANCH=${GIT_BRANCH#*/}; 
fi

docker build -t registry.walink.org/wa/$BUILD_IMAGE_NAME:$CURRENT_GIT_COMMIT -f LintingResults/Dockerfile .

docker tag registry.walink.org/wa/$BUILD_IMAGE_NAME:$CURRENT_GIT_COMMIT registry.walink.org/wa/$BUILD_IMAGE_NAME:$CURRENT_GIT_BRANCH

if [ $CURRENT_GIT_BRANCH = "master" ];
  then docker tag registry.walink.org/wa/$BUILD_IMAGE_NAME:$CURRENT_GIT_COMMIT registry.walink.org/wa/$BUILD_IMAGE_NAME:latest;
fi

docker push -a registry.walink.org/wa/$BUILD_IMAGE_NAME
