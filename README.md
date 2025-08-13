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
