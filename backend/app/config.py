from pydantic_settings import BaseSettings

class Settings(BaseSettings):
    api_key: str
    api_base: str = "https://api.deepseek.com/v1"
    
    class Config:
        env_file = ".env"

settings = Settings()
