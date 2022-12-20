#!/bin/bash

rsync -r bin/Debug/net6.0/ slipper@192.168.10.157:/home/slipper/SwippyBot/New
ssh slipper@192.168.10.157 'bash -s' < ./remotedeploybot.sh

return 0