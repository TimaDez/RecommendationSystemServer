from datetime import datetime
from typing import List, Optional, Dict

from fastapi import FastAPI
from pydantic import BaseModel


app = FastAPI(title="AI Recommendation Service")


class UserEvent(BaseModel):
    itemId: int
    eventType: str
    timestamp: Optional[datetime] = None


class RecommendationRequest(BaseModel):
    userId: int
    events: List[UserEvent]


class RecommendationItem(BaseModel):
    itemId: int
    score: float
    reason: str


class RecommendationResponse(BaseModel):
    userId: int
    recommendations: List[RecommendationItem]


@app.get("/")
def root():
    return {"status": "AI service is running"}


@app.post("/recommend", response_model=RecommendationResponse)
def recommend(req: RecommendationRequest):
    """
    Simple 'real-ish' recommendation logic:

    - Each event gives points to its item:
      - view  -> 1.0
      - click -> 2.0
      - like  -> 3.0
    - Items are ranked by total score.
    """

    if not req.events:
        # Fallback recommendations if no history
        fallback = [
            RecommendationItem(itemId=1, score=0.9, reason="popular item"),
            RecommendationItem(itemId=2, score=0.8, reason="popular item"),
        ]
        return RecommendationResponse(userId=req.userId, recommendations=fallback)

    weights: Dict[str, float] = {
        "view": 1.0,
        "click": 2.0,
        "like": 3.0,
    }

    item_scores: Dict[int, float] = {}

    for ev in req.events:
        event_type = ev.eventType.lower()
        weight = weights.get(event_type, 0.5)  # unknown types get small weight
        item_scores[ev.itemId] = item_scores.get(ev.itemId, 0.0) + weight

    # Sort items by score (highest first)
    sorted_items = sorted(item_scores.items(), key=lambda x: x[1], reverse=True)

    recommendations: List[RecommendationItem] = [
        RecommendationItem(
            itemId=item_id,
            score=score,
            reason="based on your likes/clicks/views",
        )
        for item_id, score in sorted_items[:10]
    ]

    return RecommendationResponse(
        userId=req.userId,
        recommendations=recommendations,
    )
