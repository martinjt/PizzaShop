﻿namespace StoreFront;

/// <summary>
/// The location for a courier pickup or delivery
/// </summary>
public class Address
{
    public int Id { get; set; }
		
    public string Name { get; set; } = string.Empty;

    public string Number { get; set; } = string.Empty;
    
    public int OrderId { get; set; }

    public string City { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;
}
