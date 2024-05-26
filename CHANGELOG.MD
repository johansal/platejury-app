# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.0.3] - 2024-05-26

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