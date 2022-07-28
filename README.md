# CSharpKasa

A clean and friendly library that is compatible with some Kasa smart devices including hs100 series plugs and lb100 series bulbs.



> #### **Please note:**
>
> **You'll have to have Newtonsoft.Json installed as this library is dependent on it. You can install this by going to:**
>
> - Project
> - Manage NuGet packages
> - Searching for "Newtonsoft.Json"
> - Installing either the latest version or at least version 13.0.1
> - After that is installed, try rebuilding your project - it shouldn't throw any exceptions.



#### Here's what you can do:

###### Plugs

- Turn relay state to on
- Turn relay state to off
- Get current status information
- Get current relay state (off/on)

###### Bulbs

- Change the colour synchronously

  - You only have to supply the 'Color' object - CSharpKasa will automatically convert it to a format the smart bulb recognizes.

  - You can also call this method asynchronously.
  - You can specify the brightness at which the new colour should be lit at.
  - You can specify the transition period between colour shifts as well.

- Turn bulb on

- Turn bulb off

- Get current status information



#### Examples

###### Converting the response into a workable string

```c#
        // Let's declare the location of this plug on our network
        string plugLocation = "192.168.0.2";

        // Declare new instance of the plug controller class
        communication.plug newPlugController = new communication.plug(plugLocation);

        // Turn on this plug and get response as a string
        string response = newPlugController.ToString(
           newPlugController.TurnOn()
           );
```

```c#
        // Let's declare the location of this bulb on our network
        string bulbLocation = "192.168.0.2";

        // Declare new instance of the bulb controller class
        communication.bulb newBulbController = new communication.bulb(bulbLocation);

        // Turn on this bulb and get response as a string
        string response = newBulbController.ToString(
           newBulbController.TurnOn()
           );
```

##### Plug examples

###### Getting plug state

```c#
        // Let's declare the location of this plug on our network
        string plugLocation = "192.168.0.2";

        // Declare new instance of the plug controller class
        communication.plug newPlugController = new communication.plug(plugLocation);

        // Do something if the plug is off
        if(newPlugController.State() == communication.plug.CurrentState.Off)
        {
           // Do something
        }
```

###### Turning plug on

```c#
        // Let's declare the location of this plug on our network
        string plugLocation = "192.168.0.2";

        // Declare new instance of the plug controller class
        communication.plug newPlugController = new communication.plug(plugLocation);

        // Turn on this plug
        newPlugController.TurnOn();
```

###### Turning plug off

```c#
        // Let's declare the location of this plug on our network
        string plugLocation = "192.168.0.2";

        // Declare new instance of the plug controller class
        communication.plug newPlugController = new communication.plug(plugLocation);

        // Turn off this plug
        newPlugController.TurnOff();
```

###### Getting plug status

```c#
            // Let's declare the location of this bulb on our network
            string plugLocation = "192.168.0.2";

            // Declare new instance of the bulb controller class
            communication.plug newPlugController = new communication.plug(plugLocation);

            // Request status information from device
            dynamic plugResponse = newPlugController.Information();

            // Spit response to console
            Console.WriteLine(plugResponse);
```

###### 

##### Bulb examples

###### Turning bulb on

```c#
        // Let's declare the location of this bulb on our network
        string bulbLocation = "192.168.0.2";

        // Declare new instance of the bulb controller class
        communication.bulb newBulbController = new communication.bulb(bulbLocation);

        // Turn on this bulb
        newBulbController.TurnOn();
```

###### Turning bulb off

```c#
        // Let's declare the location of this bulb on our network
        string bulbLocation = "192.168.0.2";

        // Declare new instance of the bulb controller class
        communication.bulb newBulbController = new communication.bulb(bulbLocation);

        // Turn off this bulb
        newBulbController.TurnOff();
```

###### Changing the bulb colour

```c#
        // Let's declare the location of this bulb on our network
        string bulbLocation = "192.168.0.2";

        // Let's declare the colour we want to change the bulb to - for example the colour chosen is red.
        Color newColour = Color.Red;

        // Let's declare the brightness of which we want the bulb to be lit at
        communication.bulb.Percentage newBrightness = communication.bulb.Percentage._50;

        // Let's create a new instance of the bulb controller - this gives us options on how to control this bulb
        communication.bulb newController = new communication.bulb(bulbLocation);

        // Let's declare a transition - this creates a smooth animation from one colour to another until the final colour is shown. Basically this stops the bulb skipping / strobing between colours. We'll set a quarter of a second
        communication.bulb.TransitionPeriod newTransition = communication.bulb.TransitionPeriod.Quarter_of_a_second;

        // Finally, let's set the colour of this bulb - we'll use 'async' so that we don't have to wait for the bulb to respond. This helps since if we didn't use asyncronous, we'd have to wait for the bulb to respond before we could request another colour
        newController.SetColourAsync(newColour, newBrightness, newTransition);
```

###### Getting bulb status

```c#
            // Let's declare the location of this bulb on our network
            string bulbLocation = "192.168.0.2";

            // Declare new instance of the bulb controller class
            communication.bulb newBulbController = new communication.bulb(bulbLocation);

            // Request status from device
            dynamic bulbResponse = newBulbController.Status();

            // Spit response to console
            Console.WriteLine(bulbResponse);
```

###### 

# References

This project is almost all my own code but the encrypt, decrypt functions and low level network functions that facilitate the communication between the client device and the smart device are used from:

- [iqmeta/tplink-smartplug: C# sample for TP-Link HS100 and HS110 WiFi smart plug (github.com)](https://github.com/iqmeta/tplink-smartplug)
  - [plasticrake/tplink-smarthome-api: TP-Link Smarthome WiFi API (github.com)](https://github.com/plasticrake/tplink-smarthome-api)
  - https://github.com/sausheong/hs1xxplug
  - https://www.softscheck.com/en/reverse-engineering-tp-link-hs110/
  - https://georgovassilis.blogspot.sg/2016/05/controlling-tp-link-hs100-wi-fi-smart.html

