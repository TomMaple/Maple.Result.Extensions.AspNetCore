# Maple.Result.Extensions.AspNetCore
Maps the [Maple.Result](https://github.com/TomMaple/Result) to the HTTP response (ASP.NET Core version)


# Project status: EARLY STAGE

Check also my libraries:
* [Maple.Result](https://github.com/TomMaple/Result),
* <i>Maple.Result.Extensions.Functions.Worker</i> (coming soon),
* [Maple.Result.Extensions.HttpClient](https://github.com/TomMaple/Maple.Result.Extensions.HttpClient).

# Give it a star ⭐
Do you like it? Show your support by giving this project a star!

# Status
➡️ Basic mapping without configuration.  
🔲 Integration tests.  
🔲 Configuration passed as a parameter to the extension method.  
🔲 Support for global configuration.  
🔲 Support for multiple successful codes.  
🔲 Support for Created (201) with Location header.  
🔲 Documentation.

# Example Error Response
Example
```json
{
    "type": "https://example.com/probs/out-of-credit", 
    "status": 400,
    "title": "You do not have enough credit.",
    "detail": "Your current balance is 30, but that costs 50.",
    "instance": "/accounts/12345/msgs/abc",
    "errors": [
        {
            "pointer": "#/age",
            "detail": "must be a positive integer",
            "detailTemplated": {
                "messageId":"user.details.age.mustBePositive"
            }
        },
        {
            "pointer": "#/profile/colour",
            "detail": "must be ‘green’, ‘red’ or ‘blue’",
            "detailTemplated": {
                "messageId": "user.profile.colour",
                "params": {
                    "validValueIds": [
                        "user.profile.colour.green",
                        "user.profile.colour.red",
                        "user.profile.colour.blue"
                    ]
                }
            }
        }
    ],
    "detailTemplated": {
        "messageId": "user.account.balance.tooLow",
        "params": {
            "errorCode": "UAB17",
            "accounts": [
                {
                    "title": "Main (***9456)",
                    "url": "/accounts/12345"
                },
                {
                    "title": "Main (***3357)",
                    "url": "/accounts/67890"
                }
            ],
            "currentBalance": 30,
            "requiredBalance": 50
        }
    }

}
```

## See also
* [Problem Details for HTTP APIs - RFC 7807 is dead, long live RFC 9457](https://blog.frankel.ch/problem-details-http-apis/)
* [tag URI scheme](https://en.wikipedia.org/wiki/Tag_URI_scheme)
* [RFC 1738: Uniform Resource Locators (URL)](https://datatracker.ietf.org/doc/html/rfc1738)
* [RFC 3986: Uniform Resource Identifier (URI): Generic Syntax](https://datatracker.ietf.org/doc/html/rfc3986)
* [RFC 4151: The 'tag' URI Scheme](https://datatracker.ietf.org/doc/html/rfc4151)
* [RFC 6901: JavaScript Object Notation (JSON) Pointer](https://datatracker.ietf.org/doc/html/rfc6901)
* [RFC 9457: Problem Details for HTTP APIs](https://datatracker.ietf.org/doc/html/rfc9457)

# Contribution
Please contact author: engineer(at sign)blumail(dot)me