using System;
using System.Collections.Generic;
using System.Text.Json;
using HappyTravel.Hiroshima.Common.Models;
using HappyTravel.Hiroshima.Common.Models.Accommodations.Rooms.CancellationPolicies;
using HappyTravel.Hiroshima.Common.Models.Accommodations.Rooms.OccupancyDefinitions;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HappyTravel.Hiroshima.Data.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,")
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "Accommodations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    Address = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    TextualDescription = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    Coordinates = table.Column<Point>(type: "geometry (point)", nullable: false),
                    Rating = table.Column<int>(nullable: false),
                    CheckInTime = table.Column<string>(nullable: false),
                    CheckOutTime = table.Column<string>(nullable: false),
                    Pictures = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    ContactInfo = table.Column<ContactInfo>(type: "jsonb", nullable: false),
                    PropertyType = table.Column<int>(nullable: false),
                    AccommodationAmenities = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    AdditionalInfo = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    OccupancyDefinition = table.Column<OccupancyDefinition>(type: "jsonb", nullable: false),
                    LocationId = table.Column<int>(nullable: false),
                    ContractManagerId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accommodations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookingOrders",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReferenceCode = table.Column<string>(nullable: false),
                    LanguageCode = table.Column<string>(nullable: false),
                    StatusCode = table.Column<int>(nullable: false),
                    BookingDate = table.Column<DateTime>(nullable: false),
                    CheckInDate = table.Column<DateTime>(nullable: false),
                    CheckOutDate = table.Column<DateTime>(nullable: false),
                    Nationality = table.Column<string>(nullable: false),
                    Residency = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CancellationPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoomId = table.Column<int>(nullable: false),
                    SeasonId = table.Column<int>(nullable: false),
                    Details = table.Column<List<Policy>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CancellationPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContractAccommodationRelations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContractId = table.Column<int>(nullable: false),
                    AccommodationId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractAccommodationRelations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContractManagers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdentityHash = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractManagers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ValidFrom = table.Column<DateTime>(nullable: false),
                    ValidTo = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    ContractManagerId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Code = table.Column<string>(nullable: false),
                    Name = table.Column<JsonDocument>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Locality = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    Zone = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    CountryCode = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoomAllocationRequirements",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoomId = table.Column<int>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    ReleasePeriod = table.Column<int>(nullable: false),
                    MinimumStayNights = table.Column<int>(nullable: true),
                    Allotment = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomAllocationRequirements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoomAvailabilityRestrictions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoomId = table.Column<int>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    Restrictions = table.Column<int>(nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomAvailabilityRestrictions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoomPromotionalOffers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoomId = table.Column<int>(nullable: false),
                    BookByDate = table.Column<DateTime>(nullable: false),
                    ValidFromDate = table.Column<DateTime>(nullable: false),
                    ValidToDate = table.Column<DateTime>(nullable: false),
                    DiscountPercent = table.Column<double>(nullable: false),
                    BookingCode = table.Column<string>(nullable: false),
                    Details = table.Column<JsonDocument>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomPromotionalOffers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoomRates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoomId = table.Column<int>(nullable: false),
                    Price = table.Column<decimal>(nullable: false),
                    SeasonId = table.Column<int>(nullable: false),
                    Currency = table.Column<int>(nullable: false),
                    BoardBasis = table.Column<int>(nullable: false),
                    MealPlan = table.Column<string>(nullable: true),
                    Details = table.Column<JsonDocument>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccommodationId = table.Column<int>(nullable: false),
                    Name = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    Description = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    Amenities = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    Pictures = table.Column<JsonDocument>(nullable: true),
                    OccupancyConfigurations = table.Column<List<OccupancyConfiguration>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    ContractId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accommodations_ContractManagerId",
                table: "Accommodations",
                column: "ContractManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Accommodations_Coordinates",
                table: "Accommodations",
                column: "Coordinates")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_Accommodations_LocationId",
                table: "Accommodations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CancellationPolicies_RoomId",
                table: "CancellationPolicies",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_CancellationPolicies_SeasonId",
                table: "CancellationPolicies",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractAccommodationRelations_AccommodationId",
                table: "ContractAccommodationRelations",
                column: "AccommodationId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractAccommodationRelations_ContractId",
                table: "ContractAccommodationRelations",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ContractManagerId",
                table: "Contracts",
                column: "ContractManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_CountryCode",
                table: "Locations",
                column: "CountryCode");

            migrationBuilder.CreateIndex(
                name: "IX_RoomAllocationRequirements_RoomId",
                table: "RoomAllocationRequirements",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomAvailabilityRestrictions_RoomId",
                table: "RoomAvailabilityRestrictions",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomPromotionalOffers_RoomId",
                table: "RoomPromotionalOffers",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomRates_RoomId",
                table: "RoomRates",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomRates_SeasonId",
                table: "RoomRates",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_AccommodationId",
                table: "Rooms",
                column: "AccommodationId");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_ContractId",
                table: "Seasons",
                column: "ContractId");
            
            var langFromJsonbFunctionSql = @"CREATE FUNCTION lang_from_jsonb(multilangjson jsonb, languagecode text) returns jsonb
                                            IMMUTABLE
                                            LANGUAGE plpgsql
                                            AS
                                            $$
                                            DECLARE result jsonb;
                                                available_languages text[] := '{""ar"", ""bg"", ""de"", ""el"", ""en"", ""es"", ""fr"", ""it"", ""hu"", ""pl"", ""pt"", ""ro"", ""ru"", ""sr"", ""tr""}';
                                                lowerLanguage text := lower(languageCode);
                                            BEGIN
                                                IF NOT lowerLanguage = ANY(available_languages) THEN
                                                    RAISE 'Unknown language code: %', languageCode;
                                                END IF;
        
                                                SELECT jsonb_build_object(key, value) INTO result
                                                FROM jsonb_each(multiLangJson)
                                                WHERE key = lowerLanguage;
        
                                                IF result IS NULL THEN
                                                    SELECT jsonb_build_object(key, value) INTO result
                                                    FROM jsonb_each(multiLangJson)
                                                    WHERE key = 'en';
                                                END IF;
        
                                            RETURN result;
                                            END
                                            $$;";
            
            migrationBuilder.Sql(langFromJsonbFunctionSql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accommodations");

            migrationBuilder.DropTable(
                name: "BookingOrders");

            migrationBuilder.DropTable(
                name: "CancellationPolicies");

            migrationBuilder.DropTable(
                name: "ContractAccommodationRelations");

            migrationBuilder.DropTable(
                name: "ContractManagers");

            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "RoomAllocationRequirements");

            migrationBuilder.DropTable(
                name: "RoomAvailabilityRestrictions");

            migrationBuilder.DropTable(
                name: "RoomPromotionalOffers");

            migrationBuilder.DropTable(
                name: "RoomRates");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "Seasons");
            
            migrationBuilder.Sql("DROP FUNCTION lang_from_jsonb(multilangjson jsonb, languagecode text)");
        }
    }
}
