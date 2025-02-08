from openai import OpenAI
from .config import settings
import logging

logger = logging.getLogger(__name__)

class APIClientManager:
    _instance = None

    @classmethod
    def get_client(cls):
        if cls._instance is None:
            try:
                cls._instance = OpenAI(
                    api_key=settings.api_key,
                    base_url=settings.api_base
                )
                logger.info("API client initialized successfully")
            except Exception as e:
                logger.error(f"Failed to initialize API client: {str(e)}")
                raise RuntimeError(f"Failed to initialize API client: {str(e)}")
        return cls._instance

# Global accessor function
def get_api_client():
    return APIClientManager.get_client()
