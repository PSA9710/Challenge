

This app was built for a challenge.

To run you need to change "mainNode_IP"  and "broadcastaddres" with ur public and local ip. App was tested on local network with 3 computers and i managed to create a 1 way connection with 2 different networks. In order for this app to be able to run on 2 different networks, the port 54545 needs to be open. After you compile the app, you just need to share the executable with other computers.


Usage instructions: 
You need a main machine that has the "broadcastaddress". On that machine the app needs to be open. After the app is open hit connect. If you want, then you can start the app on another machine.  The app on the secondary machine will attempt to connect to the first one. If there wasn't any succesful connection no message will be displayed. Instead the app will just run on that machine. Other machines may connect as well.

If you only opened just one app per device, all the nodes will be connected with eachother. If on one machine , there are two or more instances open, the first one will take the role of HUB.
At this point all the connections will point to the HUB. If on another machine , a new instance was opened , there will also be created a hub. Now your network has 2 HUBS. Local instances will point to the HUB on that machine. The app on the machine where is just one instance open will point to all created HUBS, respectively each HUB will have a reference to that machine as being one of it's Nodes.

If the connection is without Hubs, the messages will be sent directly to other users. If there is one or more Hubs, the 
message will be relayed trough Hubs untill it reaches end Node. This is also visible with " VIA HUB + IP" in the message.

The Hub instance can see it's nodes, and the Hubs it is connected to. The node instance can see just the Hubs it is connected to, but will interact with it's HUB only.

There's also a Refresh button, which will update all the connected users to all the open instances. The refresh button can be triggered only by a hub. ( Beware, sometimes you need to press multiple times the button or pray it offers the exact number :) ).

Upon disconnecting , if a node is disconnected, the message will be broadcasted to the whole network, and the node will just disconnect. If it's a hub that's disconnecting, a message will be broadcasted to the whole network, and all it's nodes will leave with the hub.

Note: 
1) Because the app uses the UDP connection sometimes the message might not be recieved by the end point, and all you can do is retry the connection.
2) Sometimes the app won't connect because of Windows Firewall permission. All you have to do is hit yes and reopen the app (and pray it works).
3) If you want to use the app outside the local network, Port 54545 needs to be open in your router, and ur Windows Firewall.