﻿using System;

namespace HappyTravel.Hiroshima.DirectManager.Models.Responses
{
    public readonly struct Image
    {
        public Image(Guid id, string originalName, string originalContentType, string largeImageKey, string smallImageKey, int accommodationId, int position)
        {
            Id = id;
            OriginalName = originalName;
            OriginalContentType = originalContentType;
            LargeImageKey = largeImageKey;
            SmallImageKey = smallImageKey;
            AccommodationId = accommodationId;
            Position = position;
        }

        public Guid Id { get; }
        public string OriginalName { get; }
        public string OriginalContentType { get; }
        public string LargeImageKey { get; }
        public string SmallImageKey { get; }
        public int AccommodationId { get; }
        public int Position { get; }
    }
}
