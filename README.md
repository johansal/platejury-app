# platejury-app
Blazor app for Platejury.

### PlaylistService configuration

- ClientId: spotify api client ID
- ClientSecret: spotify api client secret
- TokenURI: spotify api url to token endpoint
- PlaylistURI: spotify api url to playlists endpoint
- PlaylistId: spotify playlist id

### VotingService configuration

- Project: Firebase project name for the database
- Collection: Firebase db collection name for the votes

### Displaynames

By default the app will try to get display names for users from the spotify api.
Unfortunately every username has to be asked separately, so to speed things up and limit request amounts you can set up 
displaynames.json file to root directory with known users names (this is also handy if the user has obfuscated displayname):
```json
{
    "spotify user id": "displayname"
}
```

### How to run

#### Local

Add playlist service configuration secrets to appsettings.json or to local secretstore (dotnet user-secrets set key value).
Add displaynames.json.
Add firestore admin sdk credentials .json-file.

Run command `dotnet run`

#### Docker

Build image

docker build -t platejury-appimage .

Run container and mount secrets

docker run -d \
  -v /path/to/platejury-app-firebase-adminsdk-1234.json:/app/platejury-app-firebase-adminsdk-1234.json:ro \
  -v /path/to/appsettings.Production.json:/app/appsettings.Production.json:ro \
  -v /path/to/displaynames.json:/app/displaynames.json \
  -p 5000:5000 \
  -e ASPNETCORE_URLS=http://+:5000 \
  --name platejury-app platejury-appimage