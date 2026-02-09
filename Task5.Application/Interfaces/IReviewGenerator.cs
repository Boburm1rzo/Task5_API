using Task5.Domain.ValueObjects;

namespace Task5.Application.Interfaces;

public interface IReviewGenerator
{
    string GenerateReview(Seed64 seed, LocaleCode locale);
}
