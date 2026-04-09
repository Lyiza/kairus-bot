## Deploy на Render через Docker (ASP.NET Core 8)

### 1) Запушить проект в GitHub
- Создай репозиторий на GitHub.
- В корне проекта выполни:

```bash
git init
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin <YOUR_GITHUB_REPO_URL>
git push -u origin main
```

Важно: не коммить секреты. Если `appsettings.Development.json` уже содержит токены/ключи — удали их из репозитория и используй переменные окружения на Render.

### 2) Создать сервис на Render
- Открой Render Dashboard → **New** → **Web Service**
- Выбери **Build and deploy from a Git repository** и подключи GitHub-репозиторий
- В настройках сервиса:
  - **Environment**: Docker
  - **Branch**: `main`
  - Остальное можно оставить по умолчанию

Render сам выставит переменную `PORT` и будет проксировать трафик на неё.

### 3) Добавить Environment Variables
В Render → Service → **Environment** добавь:
- **Vk__GroupId**
- **Vk__ConfirmationCode**
- **Vk__SecretKey**
- **Vk__AccessToken**

После этого сделай **Deploy** (или дождись автодеплоя после push).

