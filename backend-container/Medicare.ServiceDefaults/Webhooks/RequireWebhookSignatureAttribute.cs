using System;

namespace Medicare.ServiceDefaults.Webhooks;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RequireWebhookSignatureAttribute : Attribute
{
    public string SignatureHeader { get; set; } = "X-Webhook-Signature";
    public string ProviderRouteParam { get; set; } = "provider";
    public bool RejectMissingSignature { get; set; } = true;
}
