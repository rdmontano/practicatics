# Publicar el repositorio y obtener la evidencia del CI

## 1. Crear el repositorio remoto

En GitHub, cree un repositorio vacío llamado `sysaqua-pagos`. No agregue README,
licencia ni `.gitignore`, porque estos archivos ya existen localmente.

## 2. Vincular y subir las ramas

Desde la carpeta extraída del proyecto, ejecute:

```bash
git remote add origin https://github.com/USUARIO/sysaqua-pagos.git
git push -u origin main
git push -u origin feature/documentar-ci
git push -u origin feature/registrar-pago
```

Cambie `USUARIO` por su nombre de usuario de GitHub.

## 3. Crear el pull request

Abra un pull request desde `feature/documentar-ci` hacia `main`. Utilice el
texto incluido en `docs/solicitud_pull_request.md`.

## 4. Capturar la evidencia

Espere a que el check **CI - Build y pruebas / Compilar y ejecutar pruebas**
aparezca en verde. Abra el detalle y capture una imagen donde se observen el
repositorio, la rama o commit y la etapa **Ejecutar pruebas automatizadas**.

