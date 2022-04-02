#!/bin/sh
if [ $# -eq 0 ]
  then
    cd ../test/Vpiska.IntegrationTests/ && docker-compose up -d
  else
    cd ../test/Vpiska.IntegrationTests/ && docker-compose down
fi