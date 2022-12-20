#!/bin/bash

#Stop the bot service
sudo systemctl stop swippybot.service

#Move New files to Running
rm -r /home/slipper/SwippyBot/Running/*
mv /home/slipper/SwippyBot/New/* /home/slipper/SwippyBot/Running

#Start the bot service
sudo systemctl start swippybot.service

exit
return 0