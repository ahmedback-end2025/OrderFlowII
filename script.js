import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '30s', target: 10 },
        { duration: '1m', target: 30 },
        { duration: '20s', target: 0 },
    ],
};

const BASE = 'http://host.docker.internal:5195';
const headers = { 'Content-Type': 'application/json' };
const GUID = /[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}/;

export default function () {
    const payload = JSON.stringify({
        customerName: `Customer_${__VU}_${__ITER}`,
        items: [
            { name: 'Item', price: 100, quantity: 1 }
        ],
    });

    const res = http.post(`${BASE}/api/Order`, payload, { headers });
    const created = check(res, {
        'Order Created': (r) => r.status === 200 || r.status === 201,
    });

    // Works whether the API returns a bare GUID or an object like { "id": "..." }
    const match = created && res.body ? String(res.body).match(GUID) : null;
    const id = match ? match[0] : null;

    if (id) {
        const getRes = http.get(`${BASE}/api/Order/${id}`);
        check(getRes, { 'Order Fetched': (r) => r.status === 200 });

        // Complete every order that was created
        const compRes = http.put(`${BASE}/api/Order/${id}/complete`, null, { headers });
        const ok = check(compRes, {
            'Order Completed': (r) => r.status === 200 || r.status === 204,
        });
        if (!ok && __ITER < 3) {
            console.log(`complete failed: status=${compRes.status} body=${compRes.body}`);
        }
    } else if (__ITER < 3) {
        console.log(`could not read order id: status=${res.status} body=${res.body}`);
    }

    sleep(0.5);
}