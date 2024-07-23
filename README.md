# Dynamo

Dynamo is an updater tool for Cloudflares &amp; Google Domains dynDNS features. It is made to run on machines that are in a NAT network and need an external call to determine their public IP address. A periodic check determines if an update is necessary before sending the request to the DNS provider API.

## Cloudflare (CF)

### Preparation

To use Dynamo with Cloudflare you need to create an API token with the permissions to edit the DNs records of all zones in which you would like to update records. You can create such a token unter `My profile > API Tokens > Create Token`. It is adviced to use a deidcated token for Dynamo with only the necessary permissions.

Make sure you create all records you would like to update in the Cloudflare Admin Console. The records need to be created before Dynamo can update them as the tool currently does not have the ability to create new records.

### Use

Run the follwing command to start Dyanmo. Make sure to replace the environment variables with your own values. The example below uses a docker volume to store the ip cache file. This is adviced to avoid unnecessary API calls.

```bash
docker volume create dynamo-data

docker run \
    --name dynamo \
    --env Dynamo__UserAgentHeader="Dynamo.DNS.Updater-myinstance/1.0" \
    --env Cloudflare__Hosts__0__Hostname=yourdomain.com \
    --env Cloudflare__Hosts__0__ZoneId=!!!CloudflareZoneId!!! \
    --env Cloudflare__ApiToken=!!!MyApiToken!!! \
    --env Cloudflare__Enabled=true \
    --volume dynamo-data:/dynamo/data \
    --restart unless-stopped \
    dlerps/dynamo:latest
```

#### Settings

Dynamo uses appsettings in json format to load configurations. That gives you the option to either pass varibales as environment variables or create a `appsettings.Production.json` file and mount it. Especially if you are planning on using Dynamo for multiple hostnames, the file version is the recommewnded option.

The follwing options are available:

```json
{
  "Cloudflare": {
    "ApiToken": "[Your Cloudflare API Token]",
    "ApiAddress": "https://api.cloudflare.com/client/v4", // optional: no need to set this unless it changes from this value in the future
    "Enabled": true, // optional: enabled by default
    "Hosts": [
      {
        "Hostname": "my.host.net",
        "ZoneId": "[The id of the zone thius hoste belongs to]",
        "RecordType": "A", // optional: default is A
        "Ttl": 120, // optional: defaults to null -> no change
        "Proxied": true // optional: defaults to null -> no change
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

##### Host Settings

Dynamo supports multiple hostnames. If passed via environment variable make sure to specify the array index correctly with `Cloudflare__Hosts__[index]__[nested-setting]`


`Cloudflare__Hosts__0__Hostname`:  Your hostname

`Cloudflare__Hosts__0__ZoneId`:  The zone id of the hostname

`Cloudflare__Hosts__0__RecordType`:  The record type (default A)

`Cloudflare__Hosts__0__Ttl`:  The TTL of the record (default null)

`Cloudflare__Hosts__0__Proxied`:  If the Cloudflare proxy features are enabled (default null)

Each variable can be set via environment variable as well. Note the double underscore syntax for nested values!

##### Dynamo settings

`Dynamo__TimeoutInMinutes`: Interval in between ip & update checks (default 5)

`Dynamo__IpCacheFile`: Location where Dynamo remembers the last ip address. **Caution: It is adviced to use a volume or host mapping to keep track of your ip over container restarts. Repeated API calls without IP change can get your user-agent and/or hostname blocked for further API calls!**

`Dynamo__UserAgentHeader`: The User-Agent header that is used to make HTTP calls. It is advices to use a unique name since bad calls can get an agent blocked permanently.

### Logging

Logging settings are not part of the appsettings and can only be specified by environment variables.

`Verbose`: Set to `true` to enable verbose/debug logging
`JsonLogFormat`: Set to `true` to enable json logging format. This is compatible with the GELF logging driver and can be used to send logs to Graylog or Seq.

## Docker Compose

An example setup using compose with an external `appsettings.json`:

`appsettings.json`:

```json
{
  
  "Dynamo": {
    "UserAgentHeader": "Dynamo.DNS.Updater-myUnqiueFlag/1.0",
    "TimeoutInMinutes": 20,
    "IpCacheFile": "/dynamo/data/custom-ipcache.txt"
  },
  "Cloudflare": {
    "ApiToken": "[Your Cloudflare API Token]",
    "ApiAddress": "https://api.cloudflare.com/client/v4", // optional: no need to set this unless it changes from this value in the future
    "Enabled": true, // optional: enabled by default
    "Hosts": [
      {
        "Hostname": "my.host.net",
        "ZoneId": "[The id of the zone thius hoste belongs to]",
        "RecordType": "A", // optional: default is A
        "Ttl": 120, // optional: defaults to null -> no change
        "Proxied": true // optional: defaults to null -> no change
      }
    ]
  },
  "GoogleDomains": {
    "Enabled": false
  }
}

```

`docker-compose.yaml`:

```yaml
services:
  dynamo:
    image: dlerps/dynamo:latest
    restart: always
    volumes:
      - ./appsettings.json:/dynamo/appsettings.Production.json
      - dynamo-data:/dynamo/data

volumes:
  dynamo-data:
```

## Google Domains (deprecated)

As of late 2023, Google Domains has discontinued to provide its services as a domain registrar and DNS provider. As of now almost all domains have been migrated to Squarespace where no API is available to update DNS records. This makes the Google Domains feature of Dynamo obsolete.'

My suggestion is to find a new DNS provider that supports dynamic DNS updates. Cloudflare is a good option and Dynamo supports it. It is possible to still use Squarespace as the registrar and Cloudflare as the DNS provider. For that consult the services documentations on how to update the NS servers in your Sqaurespace domains to use Cloudflare as your DNS provider.

The support for Google Domains will be removed in future versions of Dynamo.

### GD - Preparation

Add the hostname you would like to use as a dyanmic DNS entry in the Google Domains Admin Console and note down the auto-generated credentials.

### GD - Use

Run the follwing command to start Dyanmo. Make sure to replace the environment variables with your own values. The example below uses a docker volume to store the ip cache file. This is adviced to avoid unnecessary API calls.

```bash
docker volume create dynamo-data

docker run \
    --name dynamo \
    --env Dynamo__UserAgentHeader="Dynamo.DNS.Updater-myinstance/1.0" \
    --env GoogleDomains__Hosts__0__Hostname=yourdomain.com \
    --env GoogleDomains__Hosts__0__Username=DynDnsUsername \
    --env GoogleDomains__Hosts__0__Password=DynDnsUsername \
    --env GoogleDomains__Enabled=true \
    --volume dynamo-data:/dynamo/data \
    --restart unless-stopped \
    dlerps/dynamo:latest
```

#### GD - Settings

Dynamo uses appsettings in json format to load configurations. That gives you the option to either pass varibales as environment variables or create a `appsettings.Production.json` file and mount it. Especially if you are planning on using Dynamo for multiple hostnames, the file version is probably the better option.

The follwing options are available:

```json
{
  "GoogleDomains": {
    "Enabled": true,
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

##### GD -Host Settings

Dynamo supports multiple hostnames. If passed via environment variable make sure to specify the array index correctly with `GoogleDomains__Hosts__[index]__[nested-setting]`


`GoogleDomains__Hosts__0__Hostname`:  Your hostname

`GoogleDomains__Hosts__0__Username`:  The auto-generated dynDNS update username associated with that hostname

`GoogleDomains__Hosts__0__Password`:  The auto-generated dynDNS update password associated with that hostname

Each variable can be set via environment variable as well. Note the double underscore syntax for nested values!

## Collaboration

Feel free to open issues or pull requests. I am happy to help with any questions or problems you might have. If you like the project, please consider giving it a star.

Try to keep the code clean and follow the applied style. I am using Rider &amp; fleet with the default settings.

Future features ideas:

- IP6 support
- More DNS providers
- More logging options
- Cloudflare: Create new records (if not existing)
