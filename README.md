# Kleer

Kleer is an accounting service — see [API documentation](https://api-doc.kleer.se/).  
This project provides C# bindings to make it easier to interact with the Kleer API.

> **Om oss / About us**  
> **SE:** Vi på Devies är experter på Kleer, är **integrationspartner** med dem och hjälper gärna till med integrationer, rådgivning och vidareutveckling.  
> **EN:** We at Devies are Kleer experts and an **integration partner**. We're happy to help with integrations, advisory work, and enhancements.  
> **Kontakt / Contact:** hello@devies.se

## Generating models

The Kleer data model is defined by an XML Schema (XSD) available at  
<https://api.kleer.se/v1/xsd>.  
We keep a copy of this schema in `Kleer/doc.xsd` and use it to generate the `Kleer.Models` classes.

The GitHub Actions pipeline automatically checks whether the generated models differ from the committed ones.  
If that check fails, you can regenerate the models locally with:

```bash
curl -sSL https://api.kleer.se/v1/xsd -o Kleer/doc.xsd
rm -rf Kleer/Models
mkdir -p Kleer/Models
xscgen Kleer/doc.xsd --namespace=Kleer.Models --output Kleer/Models
```

## KleerClient

KleerClient is a wrapper around HttpClient that simplifies working with the Kleer API.

* It sets up the BaseAddress and authentication headers (X-Token).

* It provides helpers to build requests:

  * BuildRequest – bare request with default headers.

  * BuildXmlRequest<T> – request with XML-serialized content from a model.

  * BuildBinaryRequest – request for file uploads (byte array or stream).

* It provides async send methods:

  * SendAsync(HttpRequestMessage) – returns raw HttpResponseMessage.

  * SendAsync<T>(HttpRequestMessage) – sends a request and deserializes the XML response into a model.

Example usage:

```csharp
var client = new KleerClient(token, "https://api.kleer.se/v1/company/1234/");
var request = KleerClient.BuildRequest(HttpMethod.Get, "user/54321");
var user = await client.SendAsync<User>(request);
```

or for XML POSTs:

```csharp
var data = new ApproveEventsRequest { ... };
var request = KleerClient.BuildXmlRequest(HttpMethod.Post, "event/approve", data);
var result = await client.SendAsync<Ok>(request);
```

## KleerXmlSerializer

The Kleer API’s schema is defined via XSD, and `xscgen` generates C# models decorated with `[XmlType]`.
However, `xscgen` also generates duplicate `*2Redefinition` classes for schema redefinitions, which can cause the built-in `XmlSerializer` to fail.

**`KleerXmlSerializer`** works around this by:

* Automatically renaming all `*2Redefinition` classes to unique names at runtime.
* Caching `XmlSerializer` instances per type (and per assembly override set) for performance.
* Serializing with settings tuned for the Kleer API:
  * UTF-8 encoding without BOM.
  * No XML declaration (`<?xml ...?>`).
  * Suppressed `xmlns:xsi` / `xmlns:xsd` namespace attributes.

Typically, you do not need to use `KleerXmlSerializer` directly, it is used internally by `KleerClient` for both requests and responses.
But you can use it if you just want to work with models outside of HTTP:

```csharp
var xml = KleerXmlSerializer.Serialize(user);
var parsed = KleerXmlSerializer.Deserialize<User>(xml);
```

## XML vs JSON

The Kleer API supports both XML and JSON payloads.

Historically, the JSON examples in the documentation have sometimes been outdated or inconsistent, while the XML schema (XSD) is authoritative and always up to date.
Because of this, this client **defaults to XML** for both requests and responses, ensuring compatibility with the official schema-generated models.

You can still send JSON manually if needed by constructing your own `HttpRequestMessage` with `application/json` content.
If you build helpers for this, feel free to contribute them back to this repository.
