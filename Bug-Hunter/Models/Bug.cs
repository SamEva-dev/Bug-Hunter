
namespace BugHunter.Models;

public record Bug(
Guid Id,
string Type,
float X, float Y,
float VX, float VY,
int Points,
float Radius,
bool Rare
);