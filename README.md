# AI Chatbot

## Table of Contents

- [AI Chatbot](#ai-chatbot)
    - [Table of Contents](#table-of-contents)
    - [Overview](#overview)

## Overview

This is a simple AI Chat integration with the LLM Studio server, using the Open AI like API's, a simple Http Client, and
Blazor Server with C#!

## Setup

### LLM Studio

- Ensure you have the latest version of the LLM Studio installed
- Download at least one model from the `Discover` tab
- Select the `Developer` tab
- In the server settings
    - Ensure `Verbose Logging` is enabled
    - Ensure `Log Prompts and Responses` is enabled
    - Ensure `Just-in-Time Model Loading` is enabled
- Start the server

### AIChat App

- Ensure you have the latest version of .NET 9 installed
- Set section `AiChatConfig` in `appsettings.json` with the corresponding values matching your setup.
  ex.
  ```json
  {
     "AiChatConfig": {
       "BaseUri": "http://localhost:7788",
       "ChatCompletionsUrlFragment": "v1/chat/completions",
       "Model": "phi-4",
       "Role": "user",
       "MaxTokens": 4096,
       "Stream": false,
       "WaitAndRetryIntervals": [
         "00:00:02",
         "00:00:04",
         "00:00:08"
       ]
     }
   }
   ```
- Run the project: `dotnet run --project AIChat/AIChat.csproj`!

## Tips

- Once the server is running the **server url and port** can be found in the `Developer Logs` pane.
- An easy way to get the model name is to load the model(s) once and use the copy button on the left hand side of the
  loaded model. ex `phi-4`.
