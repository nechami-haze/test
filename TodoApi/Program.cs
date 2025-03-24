using Microsoft.EntityFrameworkCore; // מייבא את ספריית Entity Framework Core לעבודה עם מסדי נתונים
using Microsoft.Extensions.Options; // מייבא את ספריית Options עבור קונפיגורציות
using TodoApi; // מייבא את המודול TodoApi
using Microsoft.AspNetCore.Mvc; // מייבא את ASP.NET Core MVC

var builder = WebApplication.CreateBuilder(args); // יוצר את הבנאי של האפליקציה

// פתרון בעיית ה-CORS
builder.Services.AddCors(option => option.AddPolicy("AllowAll", // נתינת שם להרשאה
    p => p.AllowAnyOrigin() // מאפשר כל מקור
    .AllowAnyMethod() // כל מתודה - פונקציה
    .AllowAnyHeader())); // וכל כותרת פונקציה

builder.Services.AddDbContext<ToDoDbContext>(options => options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"), // מגדיר את הקשר למסד הנתונים
new MySqlServerVersion(new Version(8, 0, 21)))); // מציין את גרסת MySQL

builder.Services.AddControllers(); // מוסיף את התמיכה בבקרי MVC
// builder.Services.AddEndpointsApiExplorer(); // (מופעל אם נדרש) מוסיף תמיכה לחקר נקודות קצה

var app = builder.Build(); // בונה את האפליקציה

if (app.Environment.IsDevelopment()) // בודק אם האפליקציה נמצאת בסביבת פיתוח
{
    // הסר את השורות הקשורות ל-Swagger
}

// CORS
app.UseCors("AllowAll"); // מפעיל את מדיניות ה-CORS שנוצרה

app.MapGet("/", async (ToDoDbContext db) => // מפה את הבקשה GET לשורש
{
    return await db.Items.ToListAsync(); // מחזיר את כל הפריטים במסד הנתונים
});

// app.MapGet("/get", () => "getAll"); // (מופעל אם נדרש) מפה בקשה GET נוספת

app.MapPost("/post", async (ToDoDbContext db, Item item) => // מפה את הבקשה POST להוספת פריט חדש
{  
    db.Items.Add(item); // מוסיף את הפריט למסד הנתונים
    await db.SaveChangesAsync(); // שומר את השינויים
    return Results.Created($"/items/{item.Name}", item); // מחזיר את התוצאה עם קוד מצב 201
});

app.MapDelete("/delete/{id}", async (ToDoDbContext db, int id) => // מפה את הבקשה DELETE למחיקת פריט לפי מזהה
{
    var item = await db.Items.FindAsync(id); // מחפש את הפריט לפי המזהה
    if (item != null) 
        db.Items.Remove(item); // אם הפריט קיים, מוחק אותו
    await db.SaveChangesAsync(); // שומר את השינויים
});

app.MapPut("/update", async (ToDoDbContext db, Item updatedItem) => // מפה את הבקשה PUT לעדכון פריט קיים
{
    var item = await db.Items.FirstOrDefaultAsync(i => i.Id == updatedItem.Id); // מחפש את הפריט לפי מזהה
    if (item != null) 
        item.IsComplete = updatedItem.IsComplete; // עדכן שדות לפי הצורך
    await db.SaveChangesAsync(); // שומר את השינויים
});

app.Run(); // מפעיל את האפליקציה
