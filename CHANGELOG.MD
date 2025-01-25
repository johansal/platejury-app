# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.0.9] - 2025-01-25

### Added

- add namespaces for razor pages

### Changed

- upgrade to dotnet 9
- moved alert for successfull voting after await for results
- refactored all alerts and confirms to bootstrap modals
- validate voterId on service side

## Fixed

- clear toggle vote button state after submit

## [0.0.8] - 2025-01-21

### Added

- duplicate check for votes/results voterid's

### Changed

- clear selectedvoter after voting
- toggle vote button text and class after selection
- ensure playlist and history data is loaded only once on intialization

## [0.0.7] - 2025-01-20

### Added

- Show alert after submitting votes
- Confirm before submitting vote with empty tracklist

### Changed

- Clear tracklist after voting

## [0.0.6] - 2024-10-14

### Added

- Duplicate check for playlist entries against history data, show warning if the entry was already used earlier

## [0.0.5] - 2024-10-12

### Added

- HistoryService and views for winner table and historical tracks on new /statistics page

## [0.0.4] - 2024-07-06

### Added

- Kestrel configuration for https

### Changed

- made VotingService configurable

### Fixed

- Weeks that don't have results are now skipped instead of showing empty table in the result view.
- stop using Hsts due to http/s redirecting issues

## [0.0.3] - 2024-05-26

### Added

- View for earlier results

### Changed

- made PlaylistService configurable

## [0.0.2] - 2024-02-10

### Added

- submit votes button will show alert if voter id is not populated
- voting service will reject votes if voters own song is voted for and show an alert
- track cards have spotify link over the image

### Fixed

- removed comma from the end of wall of shame

## [0.0.1] - 2024-02-03

### Added

- wall of shame
- colorfull resulttable
- tiebreakers
- hardcoded username dictionary for regulars (== less spotify api calls)
- voting progress refresh