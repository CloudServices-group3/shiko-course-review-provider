# Shiko Course Review Provider

Small ASP.NET Core Web API provider for course reviews in Shiko LMS.

This provider handles written course reviews from logged-in users. A user can have one active review per course. Reviews are connected to a course through `CourseId` and to a user through the user id from the JWT token.

A review can only be created or updated if the same user already has a rating for the course in Course Rating Provider.

Uses EF Core with Azure SQL. Tables are stored in the `course_review` schema because my providers share the same Azure SQL database.

Connection strings, JWT settings and Course Rating Provider base URL are stored with user secrets locally and environment variables in Azure.

## Live API

### Base URL

https://shiko-course-review-provider.azurewebsites.net

The base URL is only the root address for the API. It does not have its own page, so opening it directly can return 404. Use the endpoints below instead.

### Health check

https://shiko-course-review-provider.azurewebsites.net/health

### Scalar

https://shiko-course-review-provider.azurewebsites.net/scalar

### OpenAPI JSON

https://shiko-course-review-provider.azurewebsites.net/openapi/v1.json

## Responsibility

The Course Review Provider owns:

* written course reviews
* one review per user and course
* soft delete of reviews
* review validation
* checking that the user has already rated the course before creating or updating a review

The Course Rating Provider owns rating values, rating summaries and the user rating itself.

The Course Review Provider only stores `CourseId` and `UserId`. It does not have a database relationship to Course Provider, Course Rating Provider or Auth Provider.

## Relation to Course Rating Provider

A review cannot be created or updated unless the current user already has a rating for the same course.

When a user creates or updates a review, Course Review Provider calls Course Rating Provider:

GET /api/course-ratings/{courseId}/me

The current user's Bearer token is forwarded to Course Rating Provider.

Expected behavior:

* `200 OK` means the user has rated the course
* `404 Not Found` means the user has not rated the course
* `401 Unauthorized` or `403 Forbidden` means the token is missing, invalid or not accepted
* other errors mean the rating provider could not be verified

This keeps the providers separated. Course Review Provider does not read the Course Rating Provider database directly.

## Endpoints

All endpoints require JWT Bearer auth.

GET /api/course-reviews/{courseId}

GET /api/course-reviews/{courseId}/me

POST /api/course-reviews/{courseId}

PUT /api/course-reviews/{courseId}/me

DELETE /api/course-reviews/{courseId}/me

The user id is read from the JWT token. Frontend should not send `userId` in the request body.

GET `/api/course-reviews/{courseId}` returns active reviews for the selected course.

GET `/api/course-reviews/{courseId}/me` returns the logged-in user's active review for the selected course.

POST creates a review for the logged-in user and course. The user must already have a rating for the same course.

PUT updates the logged-in user's active review. The user must still have a rating for the same course.

DELETE soft-deletes the logged-in user's review.

## Local config

Local development uses SQL Server LocalDB.

The local database connection string is stored in `appsettings.Development.json`:

ConnectionStrings:CourseReviewDatabase

Set JWT config with user secrets:

dotnet user-secrets set "Jwt:Issuer" "your-issuer" --project .\src\Shiko.CourseReviewProvider.Api

dotnet user-secrets set "Jwt:Audience" "your-audience" --project .\src\Shiko.CourseReviewProvider.Api

dotnet user-secrets set "Jwt:SigningKey" "your-signing-key" --project .\src\Shiko.CourseReviewProvider.Api

Set Course Rating Provider base URL with user secrets:

dotnet user-secrets set "CourseRatingProvider:BaseUrl" "https://shiko-course-rating-provider.azurewebsites.net" --project .\src\Shiko.CourseReviewProvider.Api

## Azure config

Azure Web App uses environment variables and app settings.

The database connection string is stored as:

CourseReviewDatabase

JWT app settings are stored as:

Jwt__Issuer

Jwt__Audience

Jwt__SigningKey

Course Rating Provider base URL is stored as:

CourseRatingProvider__BaseUrl

## Database

Tables are stored in the `course_review` schema:

* course_review.CourseReviews
* course_review.__EFMigrationsHistory

Run migrations with:

dotnet ef database update --project .\src\Shiko.CourseReviewProvider.Api --startup-project .\src\Shiko.CourseReviewProvider.Api --context CourseReviewDbContext

## Run locally

Run the API with:

dotnet run --project .\src\Shiko.CourseReviewProvider.Api --launch-profile https

Scalar opens at the URL shown in the terminal.

## Tests

The integration tests use:

* xUnit
* WebApplicationFactory
* Testcontainers with SQL Server
* EF Core migrations against the test database
* a small JWT test helper
* a fake Course Rating Client for controlling rating-provider behavior in tests

Current integration tests cover:

* health endpoint returns `200 OK`
* protected review endpoint without token returns `401 Unauthorized`
* creating a review with a user token and existing rating returns `201 Created`
* creating a review with a user token but no rating returns `409 Conflict`
* deleting a review returns `204 No Content`
* after delete, the review is no longer returned as the user's active review

Run tests with:

dotnet test

## Notes

Course Review Provider is separate from Course Rating Provider so one provider can fail without directly crashing the other provider's database or schema.

Review creation and update verify rating through HTTP instead of a shared database relationship. This is a school-level microservice compromise that keeps provider ownership clear.

Reviews use soft delete. When a user deletes a review, the row stays in the database but is marked as deleted and no longer returned as the active review.

Rating without review is allowed. Review without rating is not allowed.

If rating succeeds but review fails in the frontend feedback flow, the rating should stay saved and the user can retry the review later. There is no distributed transaction between Rating Provider and Review Provider.
