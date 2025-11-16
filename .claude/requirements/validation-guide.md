# This is a guide to implement the validations in minimal api

# Register Validation

builder.Services.AddValidation();

# Validation direct into endpoints

app.MapPost("/users", 
    ([Required] string name, [EmailAddress] string email) 
        => TypedResults.Ok("User created"));

# Disable automatic validation

app.MapPost("/users", 
    ([Required] string name, [EmailAddress] string email) 
        => TypedResults.Ok("User created"))
    .DisableValidation();

# Class validation with data annotations

using System.ComponentModel.DataAnnotations;

public class CreateUserRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }
}

# Custom Validators

public class EvenNumberAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value == null) 
        {
            return true;
        }

        if (int.TryParse(value.ToString(), out int number))
        {
            return number % 2 == 0;
        }

        return false;
    }
}

public class ProductRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; }

    [EvenNumber(ErrorMessage = "Quantity per box must be an even number")]
    public int QuantityPerBox { get; set; }
}