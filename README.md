# KatzibuildParser

Just a simple library for parsing KATZIBUILD files

## Including in your project

```
dotnet add package KatzibuildParser
```

## Usage

```
// From file
Katzibuild katzibuild = KatzibuildParser.FromFile("/file/to/katzibuild");

// From file content
Katzibuild katzibuild = KatzibuildParser.Parse("Katzibuild file content :3");
```
