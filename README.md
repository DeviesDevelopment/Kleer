# Kleer

Kleer is an accounting service, see https://api-doc.kleer.se/. This project aims to make it easier to interact with their API in C#.

## Generating documentation

```bash
curl -sSL https://api.kleer.se/v1/xsd -o Kleer/doc.xsd
rm -rf Kleer/Models
mkdir -p Kleer/Models
xscgen Kleer/doc.xsd --namespace=Kleer.Models --output Kleer/Models
```
