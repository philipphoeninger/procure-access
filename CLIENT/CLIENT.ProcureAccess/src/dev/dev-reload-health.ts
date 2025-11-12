// call this from main.ts only in development
function startApiHealthWatcher() {
  const checkUrl = 'http://localhost:5000/health';
  setInterval(async () => {
    try {
      const res = await fetch(checkUrl, { method: 'GET', cache: 'no-cache' });
      if (!res.ok) {
        console.debug('API not healthy (status ' + res.status + ')');
      }
    } catch {
      // if offline, reload to let dev server re-connect / show error
      console.debug('API unreachable ...'); //— reloading page to reconnect
      // window.location.reload();
    }
  }, 5000); // every 5s
}

export { startApiHealthWatcher };
