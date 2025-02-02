A basic C# port of Fasttext, a library to identify the languages used to write a text. 

## What and Why

This library supports the .NET Standard 2.0 and 2.1. This library exists only because I wanted a version of the library without relying on the original library in C. If you do not care about that, you most likely prefer a library that is just a wrapper of the original library. I could not find a package like that on nuget, so here we are.

**This library only supports using an existing model rather than creating a new one.**

This library distributes the [original compressed version of the model developed by Facebook](https://fasttext.cc/docs/en/language-identification.html).

Performance is good enough for the compressed version of the library. The performance for tests of the full model is bad (it takes more than an hour to run them). I am not sure why. However, using the full model the performance is as expected.

##  Installation

Install using the [NuGet](https://www.nuget.org/packages/LanguageTeller/) package.

```
PM> Install-Package LanguageTeller
```

##  Usage

Initializing the main class, `LanguageTeller`, automatically loads the default model. Then you can use:

- `TellLanguage` method, to identify the most likely language for a piece of text (or input stream).

- `TellMainLanguages` method, to identify the most likely languages of a piece of text (or input stream). By default this means languages thar are at least a 20% match. This value can be changed using the property `ThresholdMainLanguage`.

- `TellAllanguages` method, to identify all the potential languages for a piece of text (or input stream).

There are also utility method to check a model or load your own. You might also be interested in `GetLabels` which lists the labels used by a model.