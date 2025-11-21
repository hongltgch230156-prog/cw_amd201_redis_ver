<script setup>
import { computed } from 'vue';

// Nhận dữ liệu lịch sử request từ component cha truyền vào
const props = defineProps({
  logs: {
    type: Array,
    default: () => []
  },
  isOpen: Boolean
});

const emit = defineEmits(['close']);

// --- TÍNH TOÁN CÁC CHỈ SỐ (KPIs) ---
const totalRequests = computed(() => props.logs.length);

const cacheHits = computed(() => props.logs.filter(x => x.source === 'Redis').length);
const cacheMisses = computed(() => props.logs.filter(x => x.source === 'Database').length);

const hitRate = computed(() => {
  if (totalRequests.value === 0) return 0;
  return ((cacheHits.value / totalRequests.value) * 100).toFixed(2);
});

// Tính thời gian trung bình (Average Response Time)
const avgTime = computed(() => {
  if (totalRequests.value === 0) return 0;
  const sum = props.logs.reduce((acc, curr) => acc + curr.duration, 0);
  return (sum / totalRequests.value).toFixed(2);
});

// Tính thời gian trung bình riêng cho Redis vs DB (để vẽ biểu đồ so sánh)
const avgRedisTime = computed(() => {
  const redisLogs = props.logs.filter(x => x.source === 'Redis');
  if (redisLogs.length === 0) return 0;
  return (redisLogs.reduce((a, b) => a + b.duration, 0) / redisLogs.length).toFixed(2);
});

const avgDbTime = computed(() => {
  const dbLogs = props.logs.filter(x => x.source === 'Database');
  if (dbLogs.length === 0) return 0;
  return (dbLogs.reduce((a, b) => a + b.duration, 0) / dbLogs.length).toFixed(2);
});

// Giả lập thời gian tiết kiệm được (Ví dụ: DB mất 200ms, Redis mất 5ms -> Tiết kiệm 195ms mỗi request)
const timeSaved = computed(() => {
  // Giả sử trung bình DB thực tế mất khoảng 200ms nếu không có cache
  const estimatedDbTime = 200; 
  const savedMs = cacheHits.value * (estimatedDbTime - 5); // 5ms là tốc độ Redis trung bình
  return (savedMs / 1000).toFixed(2) + 's'; // Đổi ra giây
});
</script>

<template>
  <div v-if="isOpen" class="modal-overlay" @click.self="emit('close')">
    <div class="dashboard-container">
      <!-- HEADER -->
      <div class="header">
        <h2>🚀 Redis Performance Monitor</h2>
        <button @click="emit('close')" class="btn-close">✖</button>
      </div>

      <!-- 4 Ô VUÔNG NHỎ (KPIs) -->
      <div class="kpi-grid">
        <div class="card">
          <div class="label">TOTAL REQUESTS</div>
          <div class="value">{{ totalRequests }}</div>
          <div class="icon">📊</div>
        </div>
        <div class="card">
          <div class="label">CACHE HIT RATE</div>
          <div class="value highlight-green">{{ hitRate }}%</div>
          <div class="sub-label">{{ cacheHits }} Hits / {{ cacheMisses }} Misses</div>
        </div>
        <div class="card">
          <div class="label">AVG RESPONSE TIME</div>
          <div class="value">{{ avgTime }}ms</div>
          <div class="icon">⚡</div>
        </div>
        <div class="card">
          <div class="label">TIME SAVED (EST.)</div>
          <div class="value">{{ timeSaved }}</div>
          <div class="sub-label">vs Direct Database</div>
        </div>
      </div>

      <!-- 2 Ô BIỂU ĐỒ TO -->
      <div class="charts-grid">
        <!-- Response Time Comparison -->
        <div class="card chart-card">
          <h3>Response Time Comparison</h3>
          <div class="bar-container">
            <div class="bar-label">Redis (Cache)</div>
            <div class="bar-wrapper">
              <div class="bar redis-bar" :style="{ width: Math.min(avgRedisTime * 2, 100) + '%' }"></div>
              <span class="bar-value">{{ avgRedisTime }}ms</span>
            </div>
          </div>
          <div class="bar-container">
            <div class="bar-label">Database (Disk)</div>
            <div class="bar-wrapper">
              <div class="bar db-bar" :style="{ width: Math.min(avgDbTime * 0.5, 100) + '%' }"></div>
              <span class="bar-value">{{ avgDbTime }}ms</span>
            </div>
          </div>
        </div>

        <!-- Cache Hit/Miss Ratio -->
        <div class="card chart-card">
          <h3>Cache Hit/Miss Ratio</h3>
          <div class="ratio-bar">
            <div class="ratio-segment hit" :style="{ width: hitRate + '%' }"></div>
            <div class="ratio-segment miss" :style="{ width: (100 - hitRate) + '%' }"></div>
          </div>
          <div class="legend">
            <span class="dot hit"></span> Hit ({{ hitRate }}%)
            <span class="dot miss"></span> Miss ({{ (100-hitRate).toFixed(2) }}%)
          </div>
        </div>
      </div>

      <!-- BẢNG RECENT REQUESTS -->
      <div class="recent-requests card">
        <h3>Recent Requests (Log)</h3>
        <table>
          <thead>
            <tr>
              <th>Time</th>
              <th>Type</th>
              <th>Source</th>
              <th>Duration</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            <!-- Lấy 5 request gần nhất -->
            <tr v-for="(log, index) in props.logs.slice().reverse().slice(0, 5)" :key="index">
              <td>{{ log.time }}</td>
              <td>GET</td>
              <td :class="log.source === 'Redis' ? 'text-green' : 'text-blue'">{{ log.source }}</td>
              <td>{{ log.duration }}ms</td>
              <td>
                <span :class="['badge', log.source === 'Redis' ? 'badge-hit' : 'badge-miss']">
                  {{ log.source === 'Redis' ? 'HIT' : 'MISS' }}
                </span>
              </td>
            </tr>
            <tr v-if="logs.length === 0">
              <td colspan="5" style="text-align: center; color: #888;">No data yet. Try finding a link!</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* CẤU TRÚC CHUNG */
.modal-overlay {
  position: fixed; top: 0; left: 0; width: 100%; height: 100%;
  background: rgba(0, 0, 0, 0.7); backdrop-filter: blur(4px);
  display: flex; justify-content: center; align-items: center; z-index: 9999;
}
.dashboard-container {
  background: #f3f4f6; width: 900px; max-width: 95%; border-radius: 16px;
  padding: 20px; box-shadow: 0 20px 50px rgba(0,0,0,0.3);
  font-family: 'Segoe UI', sans-serif; max-height: 90vh; overflow-y: auto;
}

/* HEADER */
.header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.header h2 { margin: 0; color: #1f2937; }
.btn-close { background: none; border: none; font-size: 24px; cursor: pointer; color: #6b7280; }

/* CARDS */
.card { background: white; border-radius: 12px; padding: 15px; box-shadow: 0 2px 5px rgba(0,0,0,0.05); }
.kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 15px; margin-bottom: 20px; }
.charts-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin-bottom: 20px; }

/* KPI TEXT */
.label { font-size: 12px; font-weight: 700; color: #6b7280; margin-bottom: 5px; text-transform: uppercase; }
.value { font-size: 24px; font-weight: 800; color: #111827; }
.sub-label { font-size: 11px; color: #9ca3af; margin-top: 2px; }
.highlight-green { color: #10b981; }

/* BIỂU ĐỒ THANH (BAR CHART MÔ PHỎNG) */
.bar-container { margin-top: 10px; }
.bar-label { font-size: 12px; margin-bottom: 4px; color: #4b5563; }
.bar-wrapper { display: flex; align-items: center; gap: 10px; }
.bar { height: 8px; border-radius: 4px; transition: width 0.5s ease; }
.redis-bar { background: #10b981; } /* Xanh lá */
.db-bar { background: #3b82f6; }    /* Xanh dương */
.bar-value { font-size: 12px; font-weight: bold; color: #374151; min-width: 40px; }

/* RATIO BAR */
.ratio-bar { display: flex; height: 12px; border-radius: 6px; overflow: hidden; background: #e5e7eb; margin-top: 15px; }
.ratio-segment { height: 100%; transition: width 0.5s ease; }
.ratio-segment.hit { background: #10b981; }
.ratio-segment.miss { background: #ef4444; }
.legend { margin-top: 10px; display: flex; gap: 15px; font-size: 12px; color: #4b5563; }
.dot { width: 8px; height: 8px; border-radius: 50%; display: inline-block; margin-right: 4px; }
.dot.hit { background: #10b981; }
.dot.miss { background: #ef4444; }

/* TABLE */
table { width: 100%; border-collapse: collapse; margin-top: 10px; }
th { text-align: left; font-size: 12px; color: #6b7280; padding-bottom: 10px; border-bottom: 1px solid #e5e7eb; }
td { padding: 10px 0; font-size: 13px; border-bottom: 1px solid #f3f4f6; color: #374151; }
.text-green { color: #10b981; font-weight: 600; }
.text-blue { color: #3b82f6; font-weight: 600; }
.badge { padding: 2px 8px; border-radius: 10px; font-size: 11px; font-weight: 700; }
.badge-hit { background: #d1fae5; color: #065f46; }
.badge-miss { background: #fee2e2; color: #991b1b; }
</style>