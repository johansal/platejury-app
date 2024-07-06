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
