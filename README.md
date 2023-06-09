# Dynamo

Dynamo is an updater tool for Google Domains dynDNS feature. It is made to run on machines that are in a NAT network and need an external call to determine their public IP address. A peridic check determines if an update is necessary before sending the request to the dynDNS API.

## Preparation

Add the hostname you would like to use as a dyanmic DNS entry in the Google Domains Admin Console and note down the auto-generated credentials.

## Use

Run the follwing command to start Dyanmo. Make sure to replace the environment variables with your own values. The example below uses a docker volume to store the ip cache file. This is adviced to avoid unnecessary API calls.

```bash
docker volume create dynamo-data

docker run \
    --name dynamo
    --env Dynamo__UserAgentHeader="Dynamo.DNS.Updater-myinstance/1.0"
    --env GoogleDomains__Hosts__0__Hostname=yourdomain.com
    --env GoogleDomains__Hosts__0__Username=DynDnsUsername
    --env GoogleDomains__Hosts__0__Password=DynDnsUsername
    --volume dynamo-data:/dynamo/data
    --restart always
```

### Variables

Dynamo uses appsettings in json format to load configurations. That gives you the option to either pass varibales as environment variables or create a `appsettings.Production.json` file and mount it. Especially if you are planning on using Dynamo for multiple hostnames, the file version is probably the better option.

The follwing options are available:

```json
{
  "GoogleDomains": {
    "Hosts": [
      {
        "Hostname": "my.host.net",
        "Username": "[autogenerated-token]",
        "Password": "[autogenerated-token]"
      }
    ]
  },
  "Dynamo": {
    "UserAgentHeader": "Dynamo.DNS.Updater-myflag/1.0",
    "TimeoutInMinutes": 5,
    "IpCacheFile": "/dynamo/data/ipcache.txt"
  }
}
```

Each variable can be set via environment variable as well. Note the double underscore syntax for nested values!

#### Dynamo settings

`Dynamo__TimeoutInMinutes`: Interval in between ip & update checks (default 5)

`Dynamo__IpCacheFile`: Location where Dynamo remembers the last ip address. **Caution: It is adviced to use a volume or host mapping to keep track of your ip over container restarts. Repeated API calls without IP change can get your user-agent and/or hostname blocked for further API calls!**

`Dynamo__UserAgentHeader`: The User-Agent header that is used to make HTTP calls. It is advices to use a unique name since bad calls can get an agent blocked

#### Google Domains / Host Settings

Dynamo supports multiple hostnames. If passed via environment variable make sure to specify the array index correctly with `GoogleDomains__Hosts__[index]__[nested-setting]`


`GoogleDomains__Hosts__0__Hostname`:  Your hostname

`GoogleDomains__Hosts__0__Username`:  The auto-generated dynDNS update username associated with that hostname

`GoogleDomains__Hosts__0__Password`:  The auto-generated dynDNS update password associated with that hostname

#### Logging

Logging settings are not part of the appsettings and can only be specified by environment variables. 

`Verbose`: Set to `true` to enable verbose/debug logging
`JsonLogFormat`: Set to `true` to enable json logging format. This is compatible with the GELF logging driver and can be used to send logs to Graylog or Seq.

## Docker Compose

An example setup using compose with an external `appsettings.json`:

`appsettings.json`:

```json
{
  "GoogleDomains": {
    "Hosts": [
      {
        "Hostname": "good.de",
        "Username": "dlerps",
        "Password": "You know nothing"
      }
    ]
  },
  "Dynamo": {
    "UserAgentHeader": "Dynamo.DNS.Updater-myUnqiueFlag/1.0",
    "TimeoutInMinutes": 20,
    "IpCacheFile": "/dynamo/data/custom-ipcache.txt"
  }
}

```

`docker-compose.yaml`:

```yaml
version: '3'

services:
  dynamo:
    image: dlerps/dynamo:latest
    restart: always
    volumes:
      - ./appsettings.Test.json:/dynamo/appsettings.Production.json
      - dynamo-data:/dynamo/data

volumes:
  dynamo-data:
```

## Collaboration

Feel free to open issues or pull requests. I am happy to help with any questions or problems you might have. If you like the project, please consider giving it a star.

Try to keep the code clean and follow the applied style. I am using Rider &amp; fleet with the default settings.